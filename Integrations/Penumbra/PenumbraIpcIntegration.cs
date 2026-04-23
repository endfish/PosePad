using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;
using Penumbra.Api.IpcSubscribers;
using PosePad.Configuration;
using PosePad.Localization;
using PosePad.Models;
using PosePad.Services;

namespace PosePad.Integrations.Penumbra;

public sealed class PenumbraIpcIntegration : IPenumbraIntegration, IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(2);

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly PluginConfiguration configuration;
    private readonly IObjectTable objectTable;
    private readonly IPluginLog log;
    private readonly EmoteActionRepository emoteActionRepository;
    private readonly object syncRoot = new();
    private readonly ApiVersion apiVersion;
    private readonly GetEnabledState getEnabledState;
    private readonly GetCollection getCollection;
    private readonly GetCollectionForObject getCollectionForObject;
    private readonly GetChangedItemsForCollection getChangedItemsForCollection;
    private readonly CheckCurrentChangedItemFunc checkCurrentChangedItemFunc;
    private readonly EventSubscriber initializedSubscriber;
    private readonly EventSubscriber disposedSubscriber;
    private readonly EventSubscriber<bool> enabledChangeSubscriber;
    private readonly EventSubscriber<ModSettingChange, Guid, string, bool> modSettingChangedSubscriber;

    private IReadOnlyList<PoseActionEntry> cachedActions = [];
    private IReadOnlyDictionary<uint, PoseActionEntry>? emoteActionLookup;
    private string statusMessage;
    private DateTime nextRefreshAtUtc = DateTime.MinValue;
    private bool dirty = true;
    private bool isApiReady;
    private bool isAvailable;

    public PenumbraIpcIntegration(
        IDalamudPluginInterface pluginInterface,
        PluginConfiguration configuration,
        IObjectTable objectTable,
        IPluginLog log,
        EmoteActionRepository emoteActionRepository)
    {
        this.pluginInterface = pluginInterface;
        this.configuration = configuration;
        this.objectTable = objectTable;
        this.log = log;
        this.emoteActionRepository = emoteActionRepository;

        statusMessage = UiText.PenumbraWaiting(configuration.Language);

        apiVersion = new ApiVersion(pluginInterface);
        getEnabledState = new GetEnabledState(pluginInterface);
        getCollection = new GetCollection(pluginInterface);
        getCollectionForObject = new GetCollectionForObject(pluginInterface);
        getChangedItemsForCollection = new GetChangedItemsForCollection(pluginInterface);
        checkCurrentChangedItemFunc = new CheckCurrentChangedItemFunc(pluginInterface);

        initializedSubscriber = Initialized.Subscriber(pluginInterface, MarkDirty);
        disposedSubscriber = Disposed.Subscriber(pluginInterface, OnDisposed);
        enabledChangeSubscriber = EnabledChange.Subscriber(pluginInterface, _ => MarkDirty());
        modSettingChangedSubscriber = ModSettingChanged.Subscriber(pluginInterface, (_, _, _, _) => MarkDirty());
    }

    public bool IsAvailable
    {
        get
        {
            lock (syncRoot)
            {
                EnsureFresh();
                return isAvailable;
            }
        }
    }

    public string StatusMessage
    {
        get
        {
            lock (syncRoot)
            {
                EnsureFresh();
                return statusMessage;
            }
        }
    }

    public IReadOnlyList<PoseActionEntry> GetModActions()
    {
        lock (syncRoot)
        {
            EnsureFresh();
            return cachedActions;
        }
    }

    public void Dispose()
    {
        initializedSubscriber.Dispose();
        disposedSubscriber.Dispose();
        enabledChangeSubscriber.Dispose();
        modSettingChangedSubscriber.Dispose();
    }

    private void EnsureFresh()
    {
        if (!dirty && DateTime.UtcNow < nextRefreshAtUtc)
            return;

        RefreshUnsafe();
    }

    private void RefreshUnsafe()
    {
        dirty = false;
        nextRefreshAtUtc = DateTime.UtcNow + RefreshInterval;

        try
        {
            _ = apiVersion.Invoke();
            isApiReady = true;
        }
        catch (Exception ex)
        {
            SetUnavailable(UiText.PenumbraUnavailable(configuration.Language), ex);
            return;
        }

        try
        {
            if (!getEnabledState.Invoke())
            {
                isAvailable = false;
                cachedActions = [];
                statusMessage = UiText.PenumbraDisabled(configuration.Language);
                return;
            }

            if (!TryResolveCurrentCollection(out var collectionId, out var collectionName))
            {
                isAvailable = false;
                cachedActions = [];
                statusMessage = UiText.PenumbraCollectionUnavailable(configuration.Language);
                return;
            }

            var changedItems = getChangedItemsForCollection.Invoke(collectionId);
            var modLookup = checkCurrentChangedItemFunc.Invoke();
            var actions = BuildActions(changedItems, modLookup);

            cachedActions = actions;
            isAvailable = true;
            statusMessage = actions.Count > 0
                ? UiText.PenumbraDetectedModActions(configuration.Language, actions.Count, collectionName)
                : UiText.PenumbraNoModActions(configuration.Language, collectionName);
        }
        catch (Exception ex)
        {
            SetUnavailable(UiText.PenumbraRefreshFailed(configuration.Language), ex);
        }
    }

    private bool TryResolveCurrentCollection(out Guid collectionId, out string collectionName)
    {
        collectionId = Guid.Empty;
        collectionName = string.Empty;

        if (objectTable.LocalPlayer is IGameObject localPlayer)
        {
            var (objectValid, _, effectiveCollection) = getCollectionForObject.Invoke(localPlayer.ObjectIndex);
            if (objectValid)
            {
                collectionId = effectiveCollection.Id;
                collectionName = effectiveCollection.Name;
                return true;
            }
        }

        var assignedCollection = getCollection.Invoke(ApiCollectionType.Yourself)
            ?? getCollection.Invoke(ApiCollectionType.Default);
        if (assignedCollection is not { } collection)
            return false;

        collectionId = collection.Id;
        collectionName = collection.Name;
        return true;
    }

    private IReadOnlyList<PoseActionEntry> BuildActions(
        IReadOnlyDictionary<string, object?> changedItems,
        Func<string, (string ModDirectory, string ModName)[]> modLookup)
    {
        var actions = new List<PoseActionEntry>(changedItems.Count);
        foreach (var (changedItemName, payload) in changedItems.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (payload is not Emote emote || emote.RowId is 0 or > ushort.MaxValue)
                continue;

            var mods = modLookup(changedItemName)
                .Select(pair => string.IsNullOrWhiteSpace(pair.ModName) ? pair.ModDirectory : pair.ModName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (mods.Count == 0)
                continue;

            var baseAction = FindBaseEmoteAction(emote.RowId);
            var actionName = !string.IsNullOrWhiteSpace(baseAction?.Name)
                ? baseAction.Name
                : emote.Name.ToString().Trim();
            if (string.IsNullOrWhiteSpace(actionName))
                continue;

            var modLabel = BuildModLabel(mods);
            actions.Add(new PoseActionEntry(
                $"penumbra:{changedItemName}",
                actionName,
                $"Emote #{emote.RowId}",
                ActionExecutionKind.Emote,
                emote.RowId,
                baseAction?.GPoseTimelineId,
                modLabel,
                ActionTabKind.Mod,
                false,
                baseAction?.IconId ?? emote.Icon,
                $"{actionName} {changedItemName} {emote.RowId} {string.Join(' ', mods)}"));
        }

        return actions;
    }

    private PoseActionEntry? FindBaseEmoteAction(uint rowId)
    {
        emoteActionLookup ??= emoteActionRepository.GetActions()
            .GroupBy(action => action.SourceId)
            .ToDictionary(group => group.Key, group => group.First());

        return emoteActionLookup.GetValueOrDefault(rowId);
    }

    private static string BuildModLabel(IReadOnlyList<string> mods)
    {
        return mods.Count switch
        {
            0 => string.Empty,
            1 => mods[0],
            2 => $"{mods[0]} / {mods[1]}",
            _ => $"{mods[0]} +{mods.Count - 1}",
        };
    }

    private void MarkDirty()
    {
        lock (syncRoot)
        {
            dirty = true;
            nextRefreshAtUtc = DateTime.MinValue;
        }
    }

    private void OnDisposed()
    {
        lock (syncRoot)
        {
            isApiReady = false;
            isAvailable = false;
            cachedActions = [];
            statusMessage = UiText.PenumbraUnavailable(configuration.Language);
            dirty = true;
            nextRefreshAtUtc = DateTime.MinValue;
        }
    }

    private void SetUnavailable(string message, Exception ex)
    {
        isAvailable = false;
        cachedActions = [];
        statusMessage = message;

        if (isApiReady)
            log.Warning(ex, "Failed to refresh Penumbra mod actions.");
    }
}
