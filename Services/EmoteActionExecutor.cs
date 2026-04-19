using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using PosePad.Configuration;
using PosePad.Localization;
using PosePad.Models;

namespace PosePad.Services;

public sealed class EmoteActionExecutor
{
    private readonly PluginConfiguration configuration;
    private readonly ActorResolverService actorResolverService;
    private readonly TimelinePlaybackService timelinePlaybackService;
    private readonly Dalamud.Plugin.Services.IPluginLog log;

    public EmoteActionExecutor(
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
        if (action.ExecutionKind != ActionExecutionKind.Emote)
            return new ActionExecutionResult(false, "This action is not an emote.");

        return actorResolverService.IsInGPose
            ? ExecuteAsGPoseTimeline(action)
            : ExecuteAsStandardEmote(action);
    }

    private unsafe ActionExecutionResult ExecuteAsStandardEmote(PoseActionEntry action)
    {
        if (action.SourceId > ushort.MaxValue)
            return new ActionExecutionResult(false, UiText.InvalidEmote(configuration.Language, action.SourceId));

        var emoteId = (ushort)action.SourceId;
        var actionName = ActionText.Name(configuration.Language, action);

        try
        {
            var uiState = UIState.Instance();
            if (uiState != null && !uiState->IsEmoteUnlocked(emoteId))
                return new ActionExecutionResult(false, UiText.EmoteNotUnlocked(configuration.Language, actionName));

            var agentModule = AgentModule.Instance();
            if (agentModule != null)
            {
                var agent = (AgentEmote*)agentModule->GetAgentByInternalId(AgentId.Emote);
                if (agent != null && agent->CanUseEmote(emoteId))
                {
                    agent->ExecuteEmote(emoteId);
                    return new ActionExecutionResult(true, UiText.ExecutedEmote(configuration.Language, actionName));
                }
            }

            return new ActionExecutionResult(false, UiText.FailedEmote(configuration.Language, actionName));
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to execute emote {EmoteId} ({Name}).", emoteId, action.Name);
            return new ActionExecutionResult(false, UiText.FailedEmote(configuration.Language, actionName));
        }
    }

    private unsafe ActionExecutionResult ExecuteAsGPoseTimeline(PoseActionEntry action)
    {
        var actionName = ActionText.Name(configuration.Language, action);

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
            log.Error(ex, "Failed to apply GPose emote timeline {TimelineId} for {Name}.", timelineId, action.Name);
            return new ActionExecutionResult(false, UiText.FailedEmote(configuration.Language, actionName));
        }
    }
}
