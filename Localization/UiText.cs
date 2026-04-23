namespace PosePad.Localization;

public static class UiText
{
    public static string MainTitle(UiLanguage language)
        => "PosePad";

    public static string Settings(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "设置",
            UiLanguage.Japanese => "設定",
            _ => "Settings",
        };

    public static string SearchHint(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "按名称或编号搜索",
            UiLanguage.Japanese => "名前またはIDで検索",
            _ => "Search by name or ID",
        };

    public static string CommonActions(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "常用动作",
            UiLanguage.Japanese => "よく使う動作",
            _ => "Common Actions",
        };

    public static string Emotes(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "情感动作",
            UiLanguage.Japanese => "エモート",
            _ => "Emotes",
        };

    public static string Expressions(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "表情",
            UiLanguage.Japanese => "表情",
            _ => "Expressions",
        };

    public static string Favorites(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "收藏动作",
            UiLanguage.Japanese => "お気に入り",
            _ => "Favorites",
        };

    public static string ModActions(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "Mod 动作",
            UiLanguage.Japanese => "Mod 動作",
            _ => "Mod Actions",
        };

    public static string Recent(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "最近使用",
            UiLanguage.Japanese => "最近使用",
            _ => "Recent",
        };

    public static string Ready(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "准备就绪。",
            UiLanguage.Japanese => "準備完了。",
            _ => "Ready.",
        };

    public static string NoRecent(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "暂无最近使用的情感动作。",
            UiLanguage.Japanese => "最近使用したエモートはありません。",
            _ => "No recent emotes.",
        };

    public static string NoFilterResults(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "当前筛选条件下没有动作。",
            UiLanguage.Japanese => "現在の条件に一致する動作はありません。",
            _ => "No actions match the current filter.",
        };

    public static string Use(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "使用",
            UiLanguage.Japanese => "使う",
            _ => "Use",
        };

    public static string CancelAction(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "取消动作",
            UiLanguage.Japanese => "動作を解除",
            _ => "Cancel Action",
        };

    public static string Favorite(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "收藏",
            UiLanguage.Japanese => "お気に入り",
            _ => "Fav",
        };

    public static string Unfavorite(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "取消收藏",
            UiLanguage.Japanese => "解除",
            _ => "Unfav",
        };

    public static string AddedToFavorites(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"已收藏：{actionName}",
            UiLanguage.Japanese => $"お気に入りに追加: {actionName}",
            _ => $"Added to favorites: {actionName}",
        };

    public static string RemovedFromFavorites(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"已取消收藏：{actionName}",
            UiLanguage.Japanese => $"お気に入りから削除: {actionName}",
            _ => $"Removed from favorites: {actionName}",
        };

    public static string SettingsWindowTitle(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "PosePad 设置",
            UiLanguage.Japanese => "PosePad 設定",
            _ => "PosePad Settings",
        };

    public static string Language(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "语言",
            UiLanguage.Japanese => "言語",
            _ => "Language",
        };

    public static string LanguageOption(UiLanguage option)
        => option switch
        {
            UiLanguage.Chinese => "中文",
            UiLanguage.Japanese => "日本語",
            _ => "English",
        };

    public static string FavoriteCount(UiLanguage language, int count)
        => language switch
        {
            UiLanguage.Chinese => $"收藏动作：{count}",
            UiLanguage.Japanese => $"お気に入り数: {count}",
            _ => $"Favorite actions: {count}",
        };

    public static string RecentCount(UiLanguage language, int count)
        => language switch
        {
            UiLanguage.Chinese => $"最近使用：{count}",
            UiLanguage.Japanese => $"最近使用数: {count}",
            _ => $"Recent actions: {count}",
        };

    public static string PenumbraIntegration(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "Penumbra 联动",
            UiLanguage.Japanese => "Penumbra 連携",
            _ => "Penumbra Integration",
        };

    public static string ClearRecent(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "清空最近使用",
            UiLanguage.Japanese => "最近使用を消去",
            _ => "Clear Recent",
        };

    public static string DisableGposeUiHide(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "在拍照模式下保持窗口显示",
            UiLanguage.Japanese => "GPose中でもUIを表示したままにする",
            _ => "Keep UI visible in GPose",
        };

    public static string InvalidEmote(UiLanguage language, uint id)
        => language switch
        {
            UiLanguage.Chinese => $"无效的情感动作编号：{id}",
            UiLanguage.Japanese => $"無効なエモートID: {id}",
            _ => $"Invalid emote id: {id}.",
        };

    public static string EmoteNotUnlocked(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"当前角色尚未解锁：{actionName}",
            UiLanguage.Japanese => $"このキャラクターでは未開放です: {actionName}",
            _ => $"{actionName} is not unlocked on this character.",
        };

    public static string ExecutedEmote(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"已执行情感动作：{actionName}",
            UiLanguage.Japanese => $"エモートを実行しました: {actionName}",
            _ => $"Executed emote: {actionName}",
        };

    public static string FailedEmote(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"执行情感动作失败：{actionName}",
            UiLanguage.Japanese => $"エモートの実行に失敗しました: {actionName}",
            _ => $"Failed to execute emote: {actionName}",
        };

    public static string InvalidTimeline(UiLanguage language, uint id)
        => language switch
        {
            UiLanguage.Chinese => $"无效的姿势编号：{id}",
            UiLanguage.Japanese => $"無効なタイムラインID: {id}",
            _ => $"Invalid timeline id: {id}.",
        };

    public static string LocalPlayerUnavailable(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "当前无法获取本地角色。",
            UiLanguage.Japanese => "ローカルプレイヤーを取得できません。",
            _ => "Local player is not available.",
        };

    public static string AppliedPose(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"已应用姿势：{actionName}",
            UiLanguage.Japanese => $"ポーズを適用しました: {actionName}",
            _ => $"Applied pose: {actionName}",
        };

    public static string FailedPose(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"应用姿势失败：{actionName}",
            UiLanguage.Japanese => $"ポーズの適用に失敗しました: {actionName}",
            _ => $"Failed to apply pose: {actionName}",
        };

    public static string TimelineOnlyInGPose(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "该类姿势只能在拍照模式中使用。",
            UiLanguage.Japanese => "この種類のポーズはGPose中のみ使用できます。",
            _ => "Timeline poses can only be applied while GPose is active.",
        };

    public static string NoGPoseActor(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "未找到可用的拍照模式目标，请重新选择目标或重开拍照模式后再试。",
            UiLanguage.Japanese => "使用可能なGPose対象が見つかりません。対象を選び直すか、GPoseを開き直してから再試行してください。",
            _ => "No GPose target actor was found. Select a GPose target or reopen GPose and try again.",
        };

    public static string NoGPoseTimeline(UiLanguage language, string actionName)
        => language switch
        {
            UiLanguage.Chinese => $"未找到 {actionName} 可用的拍照模式动画。",
            UiLanguage.Japanese => $"{actionName} に使用できるGPoseアニメーションが見つかりません。",
            _ => $"No usable GPose animation timeline was found for {actionName}.",
        };

    public static string AppliedToTarget(UiLanguage language, string actionName, string targetName)
        => language switch
        {
            UiLanguage.Chinese => $"已将 {actionName} 应用到 {targetName}。",
            UiLanguage.Japanese => $"{actionName} を {targetName} に適用しました。",
            _ => $"Applied {actionName} to {targetName}.",
        };

    public static string OpenOnEnterGPose(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "进入拍照模式时自动打开 PosePad",
            UiLanguage.Japanese => "GPose開始時にPosePadを自動で開く",
            _ => "Open PosePad automatically when entering GPose",
        };

    public static string RestoredDefaultAction(UiLanguage language, string targetName)
        => language switch
        {
            UiLanguage.Chinese => $"已恢复 {targetName} 的默认动作。",
            UiLanguage.Japanese => $"{targetName} の既定動作を復元しました。",
            _ => $"Restored the default action for {targetName}.",
        };

    public static string FailedToRestoreDefaultAction(UiLanguage language, string targetName)
        => language switch
        {
            UiLanguage.Chinese => $"恢复 {targetName} 的默认动作失败。",
            UiLanguage.Japanese => $"{targetName} の既定動作の復元に失敗しました。",
            _ => $"Failed to restore the default action for {targetName}.",
        };

    public static string PenumbraWaiting(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "正在等待 Penumbra 初始化。",
            UiLanguage.Japanese => "Penumbra の初期化を待っています。",
            _ => "Waiting for Penumbra to initialize.",
        };

    public static string PenumbraUnavailable(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "未检测到可用的 Penumbra。",
            UiLanguage.Japanese => "利用可能な Penumbra が見つかりません。",
            _ => "Penumbra is not available.",
        };

    public static string PenumbraDisabled(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "Penumbra 已安装，但当前处于关闭状态。",
            UiLanguage.Japanese => "Penumbra は導入されていますが、現在は無効です。",
            _ => "Penumbra is installed but currently disabled.",
        };

    public static string PenumbraCollectionUnavailable(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "暂时无法解析当前角色的 Penumbra 集合。",
            UiLanguage.Japanese => "現在のキャラクターに適用されている Penumbra コレクションを解決できません。",
            _ => "Could not resolve the active Penumbra collection for the current character.",
        };

    public static string PenumbraDetectedModActions(UiLanguage language, int count, string collectionName)
        => language switch
        {
            UiLanguage.Chinese => $"已从 {collectionName} 读取 {count} 个 Mod 动作。",
            UiLanguage.Japanese => $"{collectionName} から {count} 件の Mod 動作を読み込みました。",
            _ => $"Loaded {count} mod actions from {collectionName}.",
        };

    public static string PenumbraNoModActions(UiLanguage language, string collectionName)
        => language switch
        {
            UiLanguage.Chinese => $"{collectionName} 当前没有已启用的 Mod 动作。",
            UiLanguage.Japanese => $"{collectionName} には現在有効な Mod 動作がありません。",
            _ => $"No enabled mod actions were found in {collectionName}.",
        };

    public static string PenumbraRefreshFailed(UiLanguage language)
        => language switch
        {
            UiLanguage.Chinese => "读取 Penumbra Mod 动作失败。",
            UiLanguage.Japanese => "Penumbra の Mod 動作を読み込めませんでした。",
            _ => "Failed to read Penumbra mod actions.",
        };
}
