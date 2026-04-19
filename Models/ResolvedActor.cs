using Dalamud.Game.ClientState.Objects.Types;

namespace PosePad.Models;

public sealed record ResolvedActor(ICharacter Character, bool IsInGPose, string SourceDescription);
