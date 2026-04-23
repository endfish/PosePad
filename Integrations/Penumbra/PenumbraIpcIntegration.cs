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
    private readonly CommonActionRepository commonActionRepository;
    private readonly EmoteActionRepository emoteActionRepository;
    private readonly object syncRoot = new();
    private readonly ApiVersion apiVersion;
    private readonly GetEnabledState getEnabledState;
    private readonly GetCollection getCollection;
    private readonly GetCollectionForObject getCollectionForObject;
    private readonly GetChangedItemsForCollection getChangedItemsForCollection;
    private readonly CheckCurrentChangedItemFunc checkCurrentChangedItemFunc;
    private readonly GetGameObjectResourcePaths getGameObjectResourcePaths;
    private readonly GetModPath getModPath;
    private readonly EventSubscriber initializedSubscriber;
    private readonly EventSubscriber disposedSubscriber;
    private readonly EventSubscriber<bool> enabledChangeSubscriber;
    private readonly EventSubscriber<ModSettingChange, Guid, string, bool> modSettingChangedSubscriber;

    private IReadOnlyList<PoseActionEntry> cachedActions = [];
    private IReadOnlyDictionary<uint, PoseActionEntry>? commonActionLookup;
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
        CommonActionRepository commonActionRepository,
        EmoteActionRepository emoteActionRepository)
    {
        this.pluginInterface = pluginInterface;
        this.configuration = configuration;
        this.objectTable = objectTable;
        this.log = log;
        this.commonActionRepository = commonActionRepository;
        this.emoteActionRepository = emoteActionRepository;

        statusMessage = UiText.PenumbraWaiting(configuration.Language);

        apiVersion = new ApiVersion(pluginInterface);
        getEnabledState = new GetEnabledState(pluginInterface);
        getCollection = new GetCollection(pluginInterface);
        getCollectionForObject = new GetCollectionForObject(pluginInterface);
        getChangedItemsForCollection = new GetChangedItemsForCollection(pluginInterface);
        checkCurrentChangedItemFunc = new CheckCurrentChangedItemFunc(pluginInterface);
        getGameObjectResourcePaths = new GetGameObjectResourcePaths(pluginInterface);
        getModPath = new GetModPath(pluginInterface);

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
            var variantResources = GetVariantResources();
            var actions = BuildActions(changedItems, modLookup, variantResources);

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
        Func<string, (string ModDirectory, string ModName)[]> modLookup,
        IReadOnlyList<VariantResourceHit> variantResources)
    {
        var actions = new List<PoseActionEntry>(changedItems.Count);
        foreach (var (changedItemName, payload) in changedItems.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (payload is not Emote emote || emote.RowId is 0 or > ushort.MaxValue)
                continue;

            var mods = ResolveMods(modLookup(changedItemName));
            if (mods.Count == 0)
                continue;

            if (PenumbraPoseVariantCatalog.SupportsEmote(emote.RowId))
            {
                var variantResult = BuildVariantActions(changedItemName, emote.RowId, mods, variantResources);
                if (variantResult.Actions.Count > 0)
                {
                    actions.AddRange(variantResult.Actions);

                    var remainingMods = mods
                        .Where(mod => !variantResult.DetectedModDirectories.Contains(mod.ModDirectory))
                        .ToList();
                    if (remainingMods.Count == 0)
                        continue;

                    if (BuildGenericEmoteAction(changedItemName, emote, remainingMods) is { } remainingAction)
                        actions.Add(remainingAction);

                    continue;
                }
            }

            if (BuildGenericEmoteAction(changedItemName, emote, mods) is { } action)
                actions.Add(action);
        }

        return actions;
    }

    private IReadOnlyList<ResolvedModReference> ResolveMods((string ModDirectory, string ModName)[] modPairs)
    {
        var mods = new List<ResolvedModReference>(modPairs.Length);
        foreach (var (modDirectory, modName) in modPairs)
        {
            var displayName = string.IsNullOrWhiteSpace(modName) ? modDirectory : modName;
            if (string.IsNullOrWhiteSpace(displayName) || mods.Any(mod => string.Equals(mod.ModDirectory, modDirectory, StringComparison.OrdinalIgnoreCase)))
                continue;

            string? pathPrefix = null;
            try
            {
                var (result, fullPath, _, _) = getModPath.Invoke(modDirectory, modName);
                if (result == PenumbraApiEc.Success && !string.IsNullOrWhiteSpace(fullPath))
                    pathPrefix = NormalizeDirectoryPrefix(fullPath);
            }
            catch (Exception ex)
            {
                log.Debug(ex, "Failed to resolve Penumbra mod path for {ModDirectory}.", modDirectory);
            }

            mods.Add(new ResolvedModReference(modDirectory, displayName, pathPrefix));
        }

        return mods
            .OrderBy(mod => mod.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IReadOnlyList<VariantResourceHit> GetVariantResources()
    {
        if (objectTable.LocalPlayer is not IGameObject localPlayer)
            return [];

        try
        {
            var resources = getGameObjectResourcePaths.Invoke(localPlayer.ObjectIndex);
            if (resources.Length == 0 || resources[0] is not { } pathMap)
                return [];

            var hits = new List<VariantResourceHit>();
            foreach (var actualPath in pathMap.Keys)
            {
                if (string.IsNullOrWhiteSpace(actualPath))
                    continue;

                var fileName = Path.GetFileName(actualPath);
                if (string.IsNullOrWhiteSpace(fileName) || !PenumbraPoseVariantCatalog.TryGetVariant(fileName, out var definition))
                    continue;

                hits.Add(new VariantResourceHit(actualPath, fileName, definition));
            }

            return hits;
        }
        catch (Exception ex)
        {
            log.Debug(ex, "Failed to inspect Penumbra resource paths for pose variants.");
            return [];
        }
    }

    private VariantBuildResult BuildVariantActions(
        string changedItemName,
        uint emoteRowId,
        IReadOnlyList<ResolvedModReference> mods,
        IReadOnlyList<VariantResourceHit> variantResources)
    {
        var actions = new List<PoseActionEntry>();
        var detectedModDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var mod in mods)
        {
            if (string.IsNullOrWhiteSpace(mod.PathPrefix))
                continue;

            var hits = variantResources
                .Where(hit => hit.Definition.BaseEmoteId == emoteRowId && IsPathInDirectory(hit.ActualPath, mod.PathPrefix))
                .OrderBy(hit => hit.Definition.SortOrder)
                .ToList();
            if (hits.Count == 0)
                continue;

            foreach (var hit in hits)
            {
                var commonAction = FindCommonPoseAction(hit.Definition.TimelineId);
                if (commonAction == null)
                    continue;

                var uniqueId = $"penumbra:{mod.ModDirectory}:{hit.Definition.TimelineId}";
                if (!seen.Add(uniqueId))
                    continue;

                actions.Add(new PoseActionEntry(
                    uniqueId,
                    commonAction.Name,
                    commonAction.DetailText,
                    ActionExecutionKind.Timeline,
                    commonAction.SourceId,
                    commonAction.GPoseTimelineId,
                    mod.DisplayName,
                    ActionTabKind.Mod,
                    false,
                    commonAction.IconId,
                    $"{commonAction.Name} {changedItemName} {hit.FileName} {mod.DisplayName} {commonAction.SourceId}"));
                detectedModDirectories.Add(mod.ModDirectory);
            }
        }

        return new VariantBuildResult(actions, detectedModDirectories);
    }

    private PoseActionEntry? BuildGenericEmoteAction(
        string changedItemName,
        Emote emote,
        IReadOnlyList<ResolvedModReference> mods)
    {
        var baseAction = FindBaseEmoteAction(emote.RowId);
        var actionName = !string.IsNullOrWhiteSpace(baseAction?.Name)
            ? baseAction.Name
            : emote.Name.ToString().Trim();
        if (string.IsNullOrWhiteSpace(actionName))
            return null;

        var modLabel = BuildModLabel(mods.Select(mod => mod.DisplayName).ToList());
        return new PoseActionEntry(
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
            $"{actionName} {changedItemName} {emote.RowId} {string.Join(' ', mods.Select(mod => mod.DisplayName))}");
    }

    private PoseActionEntry? FindBaseEmoteAction(uint rowId)
    {
        emoteActionLookup ??= emoteActionRepository.GetActions()
            .GroupBy(action => action.SourceId)
            .ToDictionary(group => group.Key, group => group.First());

        return emoteActionLookup.GetValueOrDefault(rowId);
    }

    private PoseActionEntry? FindCommonPoseAction(uint rowId)
    {
        commonActionLookup ??= commonActionRepository.GetActions()
            .GroupBy(action => action.SourceId)
            .ToDictionary(group => group.Key, group => group.First());

        return commonActionLookup.GetValueOrDefault(rowId);
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

    private static string NormalizeDirectoryPrefix(string path)
    {
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);
        return $"{normalized}{Path.DirectorySeparatorChar}";
    }

    private static bool IsPathInDirectory(string actualPath, string pathPrefix)
    {
        var normalizedPath = actualPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalizedPath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase);
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

    private sealed record ResolvedModReference(string ModDirectory, string DisplayName, string? PathPrefix);

    private sealed record VariantResourceHit(
        string ActualPath,
        string FileName,
        PenumbraPoseVariantCatalog.PoseVariantDefinition Definition);

    private sealed record VariantBuildResult(
        IReadOnlyList<PoseActionEntry> Actions,
        IReadOnlySet<string> DetectedModDirectories);
}
