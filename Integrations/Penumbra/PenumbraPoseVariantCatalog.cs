using PosePad.Models;

namespace PosePad.Integrations.Penumbra;

internal static class PenumbraPoseVariantCatalog
{
    private static readonly IReadOnlyDictionary<string, PoseVariantDefinition> VariantsByFileName =
        new Dictionary<string, PoseVariantDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["s_pose01_loop.pap"] = new(50, 643, 1),
            ["s_pose02_loop.pap"] = new(50, 3132, 2),
            ["s_pose03_loop.pap"] = new(50, 3134, 3),
            ["s_pose04_loop.pap"] = new(50, 8002, 4),
            ["s_pose05_loop.pap"] = new(50, 8004, 5),
            ["j_pose01_loop.pap"] = new(52, 654, 1),
            ["j_pose02_loop.pap"] = new(52, 3136, 2),
            ["j_pose03_loop.pap"] = new(52, 3138, 3),
            ["j_pose04_loop.pap"] = new(52, 3771, 4),
            ["l_pose01_loop.pap"] = new(13, 3140, 1),
            ["l_pose02_loop.pap"] = new(13, 3142, 2),
            ["l_pose03_loop.pap"] = new(13, 585, 3),
        };

    public static bool SupportsEmote(uint emoteId)
        => VariantsByFileName.Values.Any(variant => variant.BaseEmoteId == emoteId);

    public static bool TryGetVariant(string fileName, out PoseVariantDefinition definition)
        => VariantsByFileName.TryGetValue(fileName, out definition);

    internal readonly record struct PoseVariantDefinition(uint BaseEmoteId, uint TimelineId, int SortOrder);
}
