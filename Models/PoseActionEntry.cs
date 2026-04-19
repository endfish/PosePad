namespace PosePad.Models;

public sealed record PoseActionEntry(
    string UniqueId,
    string Name,
    string DetailText,
    ActionExecutionKind ExecutionKind,
    uint SourceId,
    uint? GPoseTimelineId,
    string GroupName,
    ActionTabKind TabKind,
    bool CanFavorite,
    uint IconId,
    string SearchText);
