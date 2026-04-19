using System.Text.Json;
using PosePad.Models;

namespace PosePad.Services;

public sealed class CommonActionRepository
{
    private readonly IReadOnlyList<PoseActionEntry> actions;

    public CommonActionRepository(string dataFilePath, Dalamud.Plugin.Services.IPluginLog log)
    {
        actions = LoadActions(dataFilePath, log);
    }

    public IReadOnlyList<PoseActionEntry> GetActions()
        => actions;

    private static IReadOnlyList<PoseActionEntry> LoadActions(string dataFilePath, Dalamud.Plugin.Services.IPluginLog log)
    {
        try
        {
            if (File.Exists(dataFilePath))
            {
                var json = File.ReadAllText(dataFilePath);
                var definitions = JsonSerializer.Deserialize<List<CommonActionDefinition>>(json);
                if (definitions is { Count: > 0 })
                {
                    return definitions
                        .Select(definition => new PoseActionEntry(
                            $"timeline:{definition.Id}",
                            definition.Name,
                            $"Timeline #{definition.Id}",
                            ActionExecutionKind.Timeline,
                            definition.Id,
                            definition.Id,
                            definition.Group,
                            ActionTabKind.Common,
                            false,
                            0,
                            $"{definition.Name} {definition.Id} {definition.Group}"))
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to load common action data from {Path}. Falling back to built-in defaults.", dataFilePath);
        }

        return GetFallbackActions();
    }

    private static IReadOnlyList<PoseActionEntry> GetFallbackActions()
        => new List<PoseActionEntry>
        {
            new("timeline:643", "Chair Sit Pose 1", "Timeline #643", ActionExecutionKind.Timeline, 643, 643, "Chair Sit", ActionTabKind.Common, false, 0, "Chair Sit Pose 1 643 Chair Sit"),
            new("timeline:3132", "Chair Sit Pose 2", "Timeline #3132", ActionExecutionKind.Timeline, 3132, 3132, "Chair Sit", ActionTabKind.Common, false, 0, "Chair Sit Pose 2 3132 Chair Sit"),
            new("timeline:3134", "Chair Sit Pose 3", "Timeline #3134", ActionExecutionKind.Timeline, 3134, 3134, "Chair Sit", ActionTabKind.Common, false, 0, "Chair Sit Pose 3 3134 Chair Sit"),
            new("timeline:8002", "Chair Sit Pose 4", "Timeline #8002", ActionExecutionKind.Timeline, 8002, 8002, "Chair Sit", ActionTabKind.Common, false, 0, "Chair Sit Pose 4 8002 Chair Sit"),
            new("timeline:8004", "Chair Sit Pose 5", "Timeline #8004", ActionExecutionKind.Timeline, 8004, 8004, "Chair Sit", ActionTabKind.Common, false, 0, "Chair Sit Pose 5 8004 Chair Sit"),
            new("timeline:3", "Standing Pose 1", "Timeline #3", ActionExecutionKind.Timeline, 3, 3, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 1 3 Standing"),
            new("timeline:3124", "Standing Pose 2", "Timeline #3124", ActionExecutionKind.Timeline, 3124, 3124, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 2 3124 Standing"),
            new("timeline:3126", "Standing Pose 3", "Timeline #3126", ActionExecutionKind.Timeline, 3126, 3126, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 3 3126 Standing"),
            new("timeline:3182", "Standing Pose 4", "Timeline #3182", ActionExecutionKind.Timeline, 3182, 3182, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 4 3182 Standing"),
            new("timeline:3184", "Standing Pose 5", "Timeline #3184", ActionExecutionKind.Timeline, 3184, 3184, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 5 3184 Standing"),
            new("timeline:7405", "Standing Pose 6", "Timeline #7405", ActionExecutionKind.Timeline, 7405, 7405, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 6 7405 Standing"),
            new("timeline:7407", "Standing Pose 7", "Timeline #7407", ActionExecutionKind.Timeline, 7407, 7407, "Standing", ActionTabKind.Common, false, 0, "Standing Pose 7 7407 Standing"),
            new("timeline:654", "Ground Sit Pose 1", "Timeline #654", ActionExecutionKind.Timeline, 654, 654, "Ground Sit", ActionTabKind.Common, false, 0, "Ground Sit Pose 1 654 Ground Sit"),
            new("timeline:3136", "Ground Sit Pose 2", "Timeline #3136", ActionExecutionKind.Timeline, 3136, 3136, "Ground Sit", ActionTabKind.Common, false, 0, "Ground Sit Pose 2 3136 Ground Sit"),
            new("timeline:3138", "Ground Sit Pose 3", "Timeline #3138", ActionExecutionKind.Timeline, 3138, 3138, "Ground Sit", ActionTabKind.Common, false, 0, "Ground Sit Pose 3 3138 Ground Sit"),
            new("timeline:3771", "Ground Sit Pose 4", "Timeline #3771", ActionExecutionKind.Timeline, 3771, 3771, "Ground Sit", ActionTabKind.Common, false, 0, "Ground Sit Pose 4 3771 Ground Sit"),
            new("timeline:3140", "Sleep Pose 1", "Timeline #3140", ActionExecutionKind.Timeline, 3140, 3140, "Sleep", ActionTabKind.Common, false, 0, "Sleep Pose 1 3140 Sleep"),
            new("timeline:3142", "Sleep Pose 2", "Timeline #3142", ActionExecutionKind.Timeline, 3142, 3142, "Sleep", ActionTabKind.Common, false, 0, "Sleep Pose 2 3142 Sleep"),
            new("timeline:585", "Sleep Pose 3", "Timeline #585", ActionExecutionKind.Timeline, 585, 585, "Sleep", ActionTabKind.Common, false, 0, "Sleep Pose 3 585 Sleep"),
        };
}
