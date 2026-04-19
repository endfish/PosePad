using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace PosePad.Models;

public readonly record struct StoredTimelineState(CharacterModes Mode, byte ModeParam, ushort BaseOverride);
