using FFXIVClientStructs.FFXIV.Client.Game.Character;
using PosePad.Configuration;
using PosePad.Localization;
using PosePad.Models;

namespace PosePad.Services;

public sealed class TimelineActionExecutor
{
    private readonly PluginConfiguration configuration;
    private readonly ActorResolverService actorResolverService;
    private readonly TimelinePlaybackService timelinePlaybackService;
    private readonly Dalamud.Plugin.Services.IPluginLog log;

    public TimelineActionExecutor(
        PluginConfiguration configuration,
        ActorResolverService actorResolverService,
        TimelinePlaybackService timelinePlaybackService,
        Dalamud.Plugin.Services.IPluginLog log)
    {
        this.configuration = configuration;
        this.actorResolverService = actorResolverService;
        this.timelinePlaybackService = timelinePlaybackService;
        this.log = log;
    }

    public unsafe ActionExecutionResult Execute(PoseActionEntry action)
    {
        if (action.ExecutionKind != ActionExecutionKind.Timeline)
            return new ActionExecutionResult(false, "This action is not a timeline pose.");

        var actionName = ActionText.Name(configuration.Language, action);

        if (!actorResolverService.IsInGPose)
            return new ActionExecutionResult(false, UiText.TimelineOnlyInGPose(configuration.Language));

        if (action.GPoseTimelineId is not { } timelineId)
            return new ActionExecutionResult(false, UiText.NoGPoseTimeline(configuration.Language, actionName));

        if (timelineId > ushort.MaxValue)
            return new ActionExecutionResult(false, UiText.InvalidTimeline(configuration.Language, timelineId));

        var resolvedActor = actorResolverService.ResolvePreferredActor();
        if (resolvedActor == null)
            return new ActionExecutionResult(false, UiText.NoGPoseActor(configuration.Language));

        try
        {
            var ushortTimelineId = (ushort)timelineId;
            timelinePlaybackService.ApplyTimeline(resolvedActor, ushortTimelineId);

            return new ActionExecutionResult(
                true,
                UiText.AppliedToTarget(configuration.Language, actionName, resolvedActor.Character.Name.TextValue));
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to apply timeline {TimelineId} ({Name}).", timelineId, action.Name);
            return new ActionExecutionResult(false, UiText.FailedPose(configuration.Language, actionName));
        }
    }
}
