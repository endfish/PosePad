using PosePad.Configuration;
using PosePad.Localization;
using PosePad.Models;

namespace PosePad.Services;

public sealed class ActionExecutionService
{
    private readonly PluginConfiguration configuration;
    private readonly ActionCatalogService catalogService;
    private readonly ActorResolverService actorResolverService;
    private readonly TimelinePlaybackService timelinePlaybackService;
    private readonly EmoteActionExecutor emoteActionExecutor;
    private readonly TimelineActionExecutor timelineActionExecutor;
    private readonly Dalamud.Plugin.Services.IToastGui toastGui;
    private readonly Dalamud.Plugin.Services.IPluginLog log;

    public ActionExecutionService(
        PluginConfiguration configuration,
        ActionCatalogService catalogService,
        ActorResolverService actorResolverService,
        TimelinePlaybackService timelinePlaybackService,
        EmoteActionExecutor emoteActionExecutor,
        TimelineActionExecutor timelineActionExecutor,
        Dalamud.Plugin.Services.IToastGui toastGui,
        Dalamud.Plugin.Services.IPluginLog log)
    {
        this.configuration = configuration;
        this.catalogService = catalogService;
        this.actorResolverService = actorResolverService;
        this.timelinePlaybackService = timelinePlaybackService;
        this.emoteActionExecutor = emoteActionExecutor;
        this.timelineActionExecutor = timelineActionExecutor;
        this.toastGui = toastGui;
        this.log = log;
    }

    public ActionExecutionResult Execute(PoseActionEntry action)
    {
        var actionName = ActionText.Name(configuration.Language, action);
        var result = action.ExecutionKind switch
        {
            ActionExecutionKind.Emote => emoteActionExecutor.Execute(action),
            ActionExecutionKind.Timeline => timelineActionExecutor.Execute(action),
            _ => new ActionExecutionResult(false, $"Unsupported action kind: {action.ExecutionKind}")
        };

        if (result.Success)
        {
            if (ShouldTrackRecent(action))
                configuration.PushRecent(action.UniqueId);

            configuration.Save();
            log.Information("Executed action {ActionId} ({Name}).", action.UniqueId, actionName);
        }
        else
        {
            toastGui.ShowNormal(result.UserMessage);
            log.Warning("Execution failed for {ActionId} ({Name}): {Message}", action.UniqueId, actionName, result.UserMessage);
        }

        return result;
    }

    public ActionExecutionResult CancelCurrentAction()
    {
        var resolvedActor = actorResolverService.ResolvePreferredActor();
        if (resolvedActor == null)
        {
            var message = actorResolverService.IsInGPose
                ? UiText.NoGPoseActor(configuration.Language)
                : UiText.LocalPlayerUnavailable(configuration.Language);
            toastGui.ShowNormal(message);
            return new ActionExecutionResult(false, message);
        }

        try
        {
            timelinePlaybackService.ResetActor(resolvedActor);
            var message = UiText.RestoredDefaultAction(configuration.Language, resolvedActor.Character.Name.TextValue);
            log.Information("Restored default action for {TargetName}.", resolvedActor.Character.Name.TextValue);
            return new ActionExecutionResult(true, message);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to restore default action for {TargetName}.", resolvedActor.Character.Name.TextValue);
            var message = UiText.FailedToRestoreDefaultAction(configuration.Language, resolvedActor.Character.Name.TextValue);
            toastGui.ShowNormal(message);
            return new ActionExecutionResult(false, message);
        }
    }

    public bool ToggleFavorite(PoseActionEntry action)
    {
        if (!action.CanFavorite)
            return false;

        var nowFavorite = !configuration.IsFavorite(action.UniqueId);
        configuration.SetFavorite(action.UniqueId, nowFavorite);
        configuration.Save();
        return nowFavorite;
    }

    public bool IsFavorite(PoseActionEntry action)
        => action.CanFavorite && configuration.IsFavorite(action.UniqueId);

    public IReadOnlyList<PoseActionEntry> GetRecentActions()
        => catalogService.GetRecentActions();

    private static bool ShouldTrackRecent(PoseActionEntry action)
        => action.TabKind == ActionTabKind.Emote;
}
