using PosePad.Models;

namespace PosePad.Localization;

public static class ActionText
{
    public static string Name(UiLanguage language, PoseActionEntry action)
    {
        if (TryGetCommonPoseInfo(action.SourceId, out var category, out var index))
            return CommonPoseName(language, category, index);

        return action.Name;
    }

    public static string Group(UiLanguage language, PoseActionEntry action)
    {
        if (TryGetCommonPoseInfo(action.SourceId, out var category, out _))
            return CommonPoseGroup(language, category);

        if (action.TabKind == ActionTabKind.Expression)
            return UiText.Expressions(language);

        return action.ExecutionKind == ActionExecutionKind.Emote && action.GroupName.StartsWith("Category ", StringComparison.Ordinal)
            ? EmoteCategory(language, action.GroupName["Category ".Length..])
            : action.GroupName;
    }

    public static string Detail(UiLanguage language, PoseActionEntry action)
        => action.ExecutionKind switch
        {
            ActionExecutionKind.Timeline => TimelineDetail(language, action.SourceId),
            ActionExecutionKind.Emote => action.TabKind == ActionTabKind.Expression
                ? ExpressionDetail(language, action.SourceId)
                : EmoteDetail(language, action.SourceId),
            _ => action.DetailText,
        };

    public static string Search(UiLanguage language, PoseActionEntry action)
        => $"{Name(language, action)} {Group(language, action)} {Detail(language, action)} {action.SearchText}";

    private static string CommonPoseName(UiLanguage language, CommonPoseCategory category, int index)
        => language switch
        {
            UiLanguage.Chinese => $"{CommonPoseGroup(language, category)}{index}",
            UiLanguage.Japanese => $"{CommonPoseGroup(language, category)}{index}",
            _ => $"{CommonPoseGroup(language, category)} {index}",
        };

    private static string CommonPoseGroup(UiLanguage language, CommonPoseCategory category)
        => (language, category) switch
        {
            (UiLanguage.Chinese, CommonPoseCategory.ChairSit) => "坐姿",
            (UiLanguage.Chinese, CommonPoseCategory.Standing) => "站姿",
            (UiLanguage.Chinese, CommonPoseCategory.GroundSit) => "坐地",
            (UiLanguage.Chinese, CommonPoseCategory.Sleep) => "睡姿",
            (UiLanguage.Japanese, CommonPoseCategory.ChairSit) => "座り姿勢",
            (UiLanguage.Japanese, CommonPoseCategory.Standing) => "立ち姿勢",
            (UiLanguage.Japanese, CommonPoseCategory.GroundSit) => "地面座り",
            (UiLanguage.Japanese, CommonPoseCategory.Sleep) => "寝姿",
            (_, CommonPoseCategory.ChairSit) => "Chair Sit",
            (_, CommonPoseCategory.Standing) => "Standing",
            (_, CommonPoseCategory.GroundSit) => "Ground Sit",
            (_, CommonPoseCategory.Sleep) => "Sleep",
            _ => "Pose",
        };

    private static string TimelineDetail(UiLanguage language, uint sourceId)
        => language switch
        {
            UiLanguage.Chinese => $"动作轴 #{sourceId}",
            UiLanguage.Japanese => $"タイムライン #{sourceId}",
            _ => $"Timeline #{sourceId}",
        };

    private static string EmoteDetail(UiLanguage language, uint sourceId)
        => language switch
        {
            UiLanguage.Chinese => $"情感动作 #{sourceId}",
            UiLanguage.Japanese => $"エモート #{sourceId}",
            _ => $"Emote #{sourceId}",
        };

    private static string ExpressionDetail(UiLanguage language, uint sourceId)
        => language switch
        {
            UiLanguage.Chinese => $"表情 #{sourceId}",
            UiLanguage.Japanese => $"表情 #{sourceId}",
            _ => $"Expression #{sourceId}",
        };

    private static string EmoteCategory(UiLanguage language, string categoryId)
        => language switch
        {
            UiLanguage.Chinese => $"分类 {categoryId}",
            UiLanguage.Japanese => $"カテゴリ {categoryId}",
            _ => $"Category {categoryId}",
        };

    private static bool TryGetCommonPoseInfo(uint sourceId, out CommonPoseCategory category, out int index)
    {
        switch (sourceId)
        {
            case 643:
                category = CommonPoseCategory.ChairSit;
                index = 1;
                return true;
            case 3132:
                category = CommonPoseCategory.ChairSit;
                index = 2;
                return true;
            case 3134:
                category = CommonPoseCategory.ChairSit;
                index = 3;
                return true;
            case 8002:
                category = CommonPoseCategory.ChairSit;
                index = 4;
                return true;
            case 8004:
                category = CommonPoseCategory.ChairSit;
                index = 5;
                return true;
            case 3:
                category = CommonPoseCategory.Standing;
                index = 1;
                return true;
            case 3124:
                category = CommonPoseCategory.Standing;
                index = 2;
                return true;
            case 3126:
                category = CommonPoseCategory.Standing;
                index = 3;
                return true;
            case 3182:
                category = CommonPoseCategory.Standing;
                index = 4;
                return true;
            case 3184:
                category = CommonPoseCategory.Standing;
                index = 5;
                return true;
            case 7405:
                category = CommonPoseCategory.Standing;
                index = 6;
                return true;
            case 7407:
                category = CommonPoseCategory.Standing;
                index = 7;
                return true;
            case 654:
                category = CommonPoseCategory.GroundSit;
                index = 1;
                return true;
            case 3136:
                category = CommonPoseCategory.GroundSit;
                index = 2;
                return true;
            case 3138:
                category = CommonPoseCategory.GroundSit;
                index = 3;
                return true;
            case 3771:
                category = CommonPoseCategory.GroundSit;
                index = 4;
                return true;
            case 3140:
                category = CommonPoseCategory.Sleep;
                index = 1;
                return true;
            case 3142:
                category = CommonPoseCategory.Sleep;
                index = 2;
                return true;
            case 585:
                category = CommonPoseCategory.Sleep;
                index = 3;
                return true;
            default:
                category = default;
                index = 0;
                return false;
        }
    }

    private enum CommonPoseCategory
    {
        ChairSit,
        Standing,
        GroundSit,
        Sleep,
    }
}
