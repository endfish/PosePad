using Lumina.Excel.Sheets;
using PosePad.Models;

namespace PosePad.Services;

public sealed class EmoteActionRepository
{
    private readonly Dalamud.Plugin.Services.IDataManager dataManager;
    private readonly Dalamud.Plugin.Services.IPluginLog log;
    private IReadOnlyList<PoseActionEntry>? cachedActions;

    public EmoteActionRepository(Dalamud.Plugin.Services.IDataManager dataManager, Dalamud.Plugin.Services.IPluginLog log)
    {
        this.dataManager = dataManager;
        this.log = log;
    }

    public IReadOnlyList<PoseActionEntry> GetActions()
    {
        cachedActions ??= LoadActions();
        return cachedActions;
    }

    private IReadOnlyList<PoseActionEntry> LoadActions()
    {
        try
        {
            var sheet = dataManager.GetExcelSheet<Emote>();
            if (sheet == null)
                return [];

            return sheet
                .Where(row => row.RowId is > 0 and <= ushort.MaxValue)
                .Select(row => new
                {
                    Id = (ushort)row.RowId,
                    Name = row.Name.ToString().Trim(),
                    IconId = row.Icon,
                    CategoryId = row.EmoteCategory.RowId,
                    GPoseTimelineId = GetPrimaryGPoseTimelineId(row),
                    TabKind = GetTabKind(row.EmoteCategory.RowId),
                })
                .Where(row => !string.IsNullOrWhiteSpace(row.Name))
                .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                .Select(row => new PoseActionEntry(
                    $"emote:{row.Id}",
                    row.Name,
                    $"Emote #{row.Id}",
                    ActionExecutionKind.Emote,
                    row.Id,
                    row.GPoseTimelineId,
                    $"Category {row.CategoryId}",
                    row.TabKind,
                    true,
                    row.IconId,
                    $"{row.Name} {row.Id} {row.CategoryId}"))
                .ToList();
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to load emotes from game data.");
            return [];
        }
    }

    private static uint? GetPrimaryGPoseTimelineId(Emote row)
    {
        var candidates = new[]
        {
            row.ActionTimeline[0].RowId,
            row.ActionTimeline[1].RowId,
            row.ActionTimeline[2].RowId,
            row.ActionTimeline[3].RowId,
            row.ActionTimeline[4].RowId,
        };

        var selected = candidates.FirstOrDefault(id => id != 0);
        return selected == 0 ? null : selected;
    }

    private static ActionTabKind GetTabKind(uint emoteCategoryId)
        => emoteCategoryId == 3
            ? ActionTabKind.Expression
            : ActionTabKind.Emote;
}
