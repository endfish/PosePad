using FFXIVClientStructs.FFXIV.Client.Game.Character;
using PosePad.Models;

namespace PosePad.Services;

public sealed class TimelinePlaybackService
{
    private readonly Dictionary<nint, StoredTimelineState> originalStates = [];

    public unsafe void ApplyTimeline(ResolvedActor actor, ushort timelineId)
    {
        var address = actor.Character.Address;
        var character = (Character*)address;

        if (!originalStates.ContainsKey(address))
            originalStates[address] = new StoredTimelineState(character->Mode, character->ModeParam, character->Timeline.BaseOverride);

        character->SetMode(CharacterModes.AnimLock, 0);
        character->Timeline.BaseOverride = timelineId;
        character->Timeline.TimelineSequencer.PlayTimeline(timelineId);
    }

    public unsafe bool ResetActor(ResolvedActor actor)
    {
        var address = actor.Character.Address;
        var character = (Character*)address;

        var hadStoredState = originalStates.Remove(address, out var originalState);
        if (hadStoredState)
        {
            character->Timeline.BaseOverride = originalState.BaseOverride;
            character->Mode = originalState.Mode;
            character->ModeParam = originalState.ModeParam;
            character->Timeline.TimelineSequencer.PlayTimeline(originalState.BaseOverride == 0 ? (ushort)3 : originalState.BaseOverride);
            return true;
        }

        var hadPoseLikeState = character->Timeline.BaseOverride != 0
            || character->Mode is CharacterModes.AnimLock or CharacterModes.EmoteLoop or CharacterModes.InPositionLoop;

        character->Timeline.BaseOverride = 0;
        character->Mode = CharacterModes.Normal;
        character->ModeParam = 0;
        character->Timeline.TimelineSequencer.PlayTimeline(3);
        return hadPoseLikeState;
    }
}
