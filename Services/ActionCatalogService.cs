using PosePad.Configuration;
using PosePad.Integrations.Penumbra;
using PosePad.Models;

namespace PosePad.Services;

public sealed class ActionCatalogService
{
    private readonly CommonActionRepository commonActionRepository;
    private readonly EmoteActionRepository emoteActionRepository;
    private readonly IPenumbraIntegration penumbraIntegration;
    private readonly PluginConfiguration configuration;
    private Dictionary<string, PoseActionEntry>? actionIndex;

    public ActionCatalogService(
        CommonActionRepository commonActionRepository,
        EmoteActionRepository emoteActionRepository,
        IPenumbraIntegration penumbraIntegration,
        PluginConfiguration configuration)
    {
        this.commonActionRepository = commonActionRepository;
        this.emoteActionRepository = emoteActionRepository;
        this.penumbraIntegration = penumbraIntegration;
        this.configuration = configuration;
    }

    public IReadOnlyList<PoseActionEntry> GetCommonActions()
        => commonActionRepository.GetActions();

    public IReadOnlyList<PoseActionEntry> GetEmoteActions()
        => emoteActionRepository.GetActions()
            .Where(action => action.TabKind == ActionTabKind.Emote)
            .ToList();

    public IReadOnlyList<PoseActionEntry> GetExpressionActions()
        => emoteActionRepository.GetActions()
            .Where(action => action.TabKind == ActionTabKind.Expression)
            .ToList();

    public IReadOnlyList<PoseActionEntry> GetFavoriteActions()
        => emoteActionRepository.GetActions()
            .Where(action => configuration.IsFavorite(action.UniqueId))
            .ToList();

    public IReadOnlyList<PoseActionEntry> GetModActions()
        => penumbraIntegration.GetModActions();

    public IReadOnlyList<PoseActionEntry> GetRecentActions()
    {
        return configuration.RecentActionIds
            .Select(FindByUniqueId)
            .Where(action => action is not null)
            .Where(action => action!.TabKind == ActionTabKind.Emote)
            .Cast<PoseActionEntry>()
            .ToList();
    }

    public PoseActionEntry? FindByUniqueId(string actionId)
    {
        actionIndex ??= BuildActionIndex();
        return actionIndex.GetValueOrDefault(actionId);
    }

    private Dictionary<string, PoseActionEntry> BuildActionIndex()
    {
        return GetCommonActions()
            .Concat(emoteActionRepository.GetActions())
            .GroupBy(action => action.UniqueId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }
}
