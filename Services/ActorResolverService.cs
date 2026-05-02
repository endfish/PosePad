using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using PosePad.Models;

namespace PosePad.Services;

public sealed class ActorResolverService
{
    private const ushort GPoseObjectIndexStart = 200;
    private const ushort GPoseObjectIndexEnd = 439;

    private readonly IClientState clientState;
    private readonly IObjectTable objectTable;
    private readonly ITargetManager targetManager;
    private readonly IPluginLog log;

    public ActorResolverService(
        IClientState clientState,
        IObjectTable objectTable,
        ITargetManager targetManager,
        IPluginLog log)
    {
        this.clientState = clientState;
        this.objectTable = objectTable;
        this.targetManager = targetManager;
        this.log = log;
    }

    public bool IsInGPose
        => clientState.IsGPosing;

    public ResolvedActor? ResolvePreferredActor()
    {
        if (!IsInGPose)
            return ResolveLocalPlayer();

        if (targetManager.GPoseTarget is ICharacter targetCharacter && IsUsableCharacter(targetCharacter) && IsGPoseCharacter(targetCharacter))
            return new ResolvedActor(targetCharacter, true, "GPose target");

        var localPlayer = objectTable.LocalPlayer;
        if (localPlayer is not ICharacter localCharacter || !IsUsableCharacter(localCharacter))
            return null;

        var localName = localCharacter.Name.TextValue;
        foreach (var gameObject in objectTable)
        {
            if (gameObject is not ICharacter character || !IsUsableCharacter(character))
                continue;

            if (!IsGPoseCharacter(character) || character.ObjectKind != ObjectKind.Pc)
                continue;

            if (!string.Equals(character.Name.TextValue, localName, StringComparison.Ordinal))
                continue;

            return new ResolvedActor(character, true, "GPose self clone");
        }

        log.Warning("Failed to resolve a GPose character clone for {PlayerName}.", localName);
        return null;
    }

    private ResolvedActor? ResolveLocalPlayer()
    {
        if (objectTable.LocalPlayer is not ICharacter localCharacter || !IsUsableCharacter(localCharacter))
            return null;

        return new ResolvedActor(localCharacter, false, "Local player");
    }

    private static bool IsUsableCharacter(ICharacter character)
        => character.Address != nint.Zero;

    private static bool IsGPoseCharacter(IGameObject gameObject)
        => gameObject.ObjectIndex >= GPoseObjectIndexStart && gameObject.ObjectIndex <= GPoseObjectIndexEnd;
}
