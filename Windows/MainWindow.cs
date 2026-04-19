using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PosePad.Localization;
using PosePad.Models;

namespace PosePad.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private const float BaseTileWidth = 120f;
    private const float BaseTileHeight = 100f;
    private const float BaseTileRounding = 9f;
    private const float BaseTilePadding = 9f;
    private const float BaseFavoriteSize = 20f;
    private const int TileTitleLineLength = 8;
    private const int TileTitleMaxLines = 3;

    private readonly Plugin plugin;
    private readonly WindowStateTracker stateTracker;
    private string searchText = string.Empty;
    private string statusMessage;
    private bool statusIsError;

    public MainWindow(Plugin plugin)
        : base("PosePad###PosePadMain")
    {
        this.plugin = plugin;
        stateTracker = new WindowStateTracker(plugin.Configuration);
        statusMessage = UiText.Ready(plugin.Configuration.Language);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(900, 520),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        IsOpen = plugin.Configuration.MainWindow.IsOpen;

        if (plugin.Configuration.MainWindow.HasSize)
        {
            Size = new Vector2(plugin.Configuration.MainWindow.Width, plugin.Configuration.MainWindow.Height);
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        if (plugin.Configuration.MainWindow.HasPosition)
        {
            Position = new Vector2(plugin.Configuration.MainWindow.PositionX, plugin.Configuration.MainWindow.PositionY);
            PositionCondition = ImGuiCond.FirstUseEver;
        }
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var language = plugin.Configuration.Language;
        WindowName = $"{UiText.MainTitle(language)}###PosePadMain";

        DrawToolbar();
        ImGui.Spacing();
        DrawStatus();
        ImGui.Spacing();

        var available = ImGui.GetContentRegionAvail();
        var rightWidth = MathF.Min(280f * ImGuiHelpers.GlobalScale, available.X * 0.32f);
        var leftWidth = MathF.Max(240f, available.X - rightWidth - ImGui.GetStyle().ItemSpacing.X);

        using (var leftChild = new ChildRegion("PosePadLeftPane", new Vector2(leftWidth, 0)))
        {
            if (leftChild.Begin())
                DrawMainTabs(language);
        }

        ImGui.SameLine();

        using (var rightChild = new ChildRegion("PosePadRightPane", new Vector2(0, 0), true))
        {
            if (rightChild.Begin())
                DrawRecentPanel(language);
        }

        stateTracker.Capture(plugin.Configuration.MainWindow, IsOpen);
    }

    public override void OnClose()
    {
        plugin.Configuration.MainWindow.IsOpen = false;
        stateTracker.ForceSave(plugin.Configuration.MainWindow, false);
        base.OnClose();
    }

    private void DrawToolbar()
    {
        var language = plugin.Configuration.Language;
        ImGui.SetNextItemWidth(320 * ImGuiHelpers.GlobalScale);
        ImGui.InputTextWithHint("##PosePadSearch", UiText.SearchHint(language), ref searchText, 128);
        ImGui.SameLine();

        if (ImGui.Button(UiText.CancelAction(language)))
            CancelCurrentAction();

        ImGui.SameLine();

        if (ImGui.Button(UiText.Settings(language)))
            plugin.OpenConfigUi();
    }

    private void DrawStatus()
    {
        var color = statusIsError
            ? new Vector4(0.95f, 0.45f, 0.45f, 1.0f)
            : new Vector4(0.65f, 0.85f, 0.65f, 1.0f);

        ImGui.TextColored(color, statusMessage);
    }

    private void DrawMainTabs(UiLanguage language)
    {
        if (!ImGui.BeginTabBar("PosePadTabs"))
            return;

        if (ImGui.BeginTabItem(UiText.CommonActions(language)))
        {
            DrawCommonActions(language);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem(UiText.Emotes(language)))
        {
            DrawActionGrid(plugin.CatalogService.GetEmoteActions(), true, language);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem(UiText.Expressions(language)))
        {
            DrawActionGrid(plugin.CatalogService.GetExpressionActions(), true, language);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem(UiText.Favorites(language)))
        {
            DrawActionGrid(plugin.CatalogService.GetFavoriteActions(), true, language);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private void DrawCommonActions(UiLanguage language)
    {
        foreach (var group in plugin.CatalogService.GetCommonActions().GroupBy(action => ActionText.Group(language, action)))
        {
            if (ImGui.CollapsingHeader($"{group.Key}##{group.Key}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                DrawActionGrid(group.ToList(), false, language);
                ImGui.Spacing();
            }
        }
    }

    private void DrawActionGrid(IReadOnlyList<PoseActionEntry> actions, bool showFavoriteToggle, UiLanguage language)
    {
        var filtered = actions.Where(MatchesSearch).ToList();
        if (filtered.Count == 0)
        {
            ImGui.TextDisabled(UiText.NoFilterResults(language));
            return;
        }

        var tileWidth = BaseTileWidth * ImGuiHelpers.GlobalScale;
        var tileHeight = BaseTileHeight * ImGuiHelpers.GlobalScale;
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var contentWidth = ImGui.GetContentRegionAvail().X;
        var columns = Math.Max(1, (int)MathF.Floor((contentWidth + spacing) / (tileWidth + spacing)));

        for (var index = 0; index < filtered.Count; index++)
        {
            DrawActionTile(filtered[index], showFavoriteToggle, language, tileWidth, tileHeight);

            var isLastInRow = (index + 1) % columns == 0;
            var isLastTile = index == filtered.Count - 1;
            if (!isLastInRow && !isLastTile)
                ImGui.SameLine();
        }
    }

    private void DrawActionTile(PoseActionEntry action, bool showFavoriteToggle, UiLanguage language, float tileWidth, float tileHeight)
    {
        ImGui.PushID(action.UniqueId);

        var tileSize = new Vector2(tileWidth, tileHeight);
        var tilePadding = BaseTilePadding * ImGuiHelpers.GlobalScale;
        var tileRounding = BaseTileRounding * ImGuiHelpers.GlobalScale;
        var favoriteSize = BaseFavoriteSize * ImGuiHelpers.GlobalScale;

        ImGui.InvisibleButton("##PosePadTile", tileSize);

        var hovered = ImGui.IsItemHovered();
        var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var drawList = ImGui.GetWindowDrawList();

        var name = ActionText.Name(language, action);
        var detail = ActionText.Detail(language, action);
        var group = ActionText.Group(language, action);
        var title = BuildTileTitle(name);
        var trimmedDetail = TrimForTile(detail, 18);
        var fillTopColor = GetTileTopFillColor(action.ExecutionKind, hovered);
        var fillBottomColor = GetTileBottomFillColor(action.ExecutionKind, hovered);
        var borderColor = GetTileBorderColor(action.ExecutionKind, hovered);
        var innerBorderColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, hovered ? 0.05f : 0.03f));
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var detailColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var accentBarColor = ImGui.GetColorU32(action.ExecutionKind == ActionExecutionKind.Timeline
            ? new Vector4(0.34f, 0.62f, 0.92f, hovered ? 0.95f : 0.78f)
            : new Vector4(0.32f, 0.78f, 0.58f, hovered ? 0.95f : 0.78f));

        drawList.AddRectFilledMultiColor(min, max, fillTopColor, fillTopColor, fillBottomColor, fillBottomColor);
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(new Vector4(0.15f, 0.17f, 0.20f, 0.92f)), tileRounding);
        drawList.AddRect(min, max, borderColor, tileRounding, ImDrawFlags.None, hovered ? 1.35f : 1.0f);
        drawList.AddRect(
            min + new Vector2(1f, 1f),
            max - new Vector2(1f, 1f),
            innerBorderColor,
            Math.Max(2f, tileRounding - 1f),
            ImDrawFlags.None,
            1f);
        drawList.AddRectFilled(
            min + new Vector2(0f, 0f),
            new Vector2(max.X, min.Y + 3f * ImGuiHelpers.GlobalScale),
            accentBarColor,
            tileRounding,
            ImDrawFlags.RoundCornersTop);
        drawList.AddText(min + new Vector2(tilePadding, tilePadding), textColor, title);

        var detailSize = ImGui.CalcTextSize(trimmedDetail);
        drawList.AddText(
            new Vector2(min.X + tilePadding, max.Y - tilePadding - detailSize.Y),
            detailColor,
            trimmedDetail);

        if (hovered)
            ImGui.SetTooltip($"{name}\n{detail}\n{group}");

        var clickedFavorite = false;
        if (showFavoriteToggle && action.CanFavorite)
        {
            clickedFavorite = DrawFavoriteBadge(action, language, min, max, favoriteSize, tilePadding, tileRounding, clicked);
        }

        if (clicked && !clickedFavorite)
            ExecuteAction(action);

        ImGui.PopID();
    }

    private bool DrawFavoriteBadge(
        PoseActionEntry action,
        UiLanguage language,
        Vector2 min,
        Vector2 max,
        float favoriteSize,
        float tilePadding,
        float tileRounding,
        bool tileClicked)
    {
        var isFavorite = plugin.ExecutionService.IsFavorite(action);
        var badgeMin = new Vector2(max.X - tilePadding - favoriteSize, min.Y + tilePadding);
        var badgeMax = badgeMin + new Vector2(favoriteSize, favoriteSize);
        var mouse = ImGui.GetMousePos();
        var hovered = mouse.X >= badgeMin.X && mouse.X <= badgeMax.X && mouse.Y >= badgeMin.Y && mouse.Y <= badgeMax.Y;

        var fillColor = isFavorite
            ? ImGui.GetColorU32(new Vector4(0.88f, 0.72f, 0.20f, hovered ? 1.0f : 0.92f))
            : ImGui.GetColorU32(new Vector4(0.38f, 0.42f, 0.48f, hovered ? 0.95f : 0.78f));
        var textColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));
        var label = "F";
        var labelSize = ImGui.CalcTextSize(label);
        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(badgeMin, badgeMax, fillColor, tileRounding * 0.55f);
        drawList.AddText(
            new Vector2(
                badgeMin.X + (favoriteSize - labelSize.X) * 0.5f,
                badgeMin.Y + (favoriteSize - labelSize.Y) * 0.5f),
            textColor,
            label);

        if (hovered)
            ImGui.SetTooltip(isFavorite ? UiText.Unfavorite(language) : UiText.Favorite(language));

        if (!tileClicked || !hovered)
            return false;

        var nowFavorite = plugin.ExecutionService.ToggleFavorite(action);
        statusMessage = nowFavorite
            ? UiText.AddedToFavorites(language, ActionText.Name(language, action))
            : UiText.RemovedFromFavorites(language, ActionText.Name(language, action));
        statusIsError = false;
        return true;
    }

    private void DrawRecentPanel(UiLanguage language)
    {
        ImGui.Text(UiText.Recent(language));
        ImGui.Separator();

        var recents = plugin.ExecutionService.GetRecentActions().Where(MatchesSearch).ToList();
        if (recents.Count == 0)
        {
            ImGui.TextDisabled(UiText.NoRecent(language));
            return;
        }

        foreach (var action in recents)
        {
            ImGui.PushID($"recent:{action.UniqueId}");
            if (ImGui.SmallButton(UiText.Use(language)))
                ExecuteAction(action);

            ImGui.SameLine();
            ImGui.TextWrapped(ActionText.Name(language, action));
            ImGui.TextDisabled(ActionText.Detail(language, action));
            ImGui.Separator();
            ImGui.PopID();
        }
    }

    private bool MatchesSearch(PoseActionEntry action)
        => string.IsNullOrWhiteSpace(searchText)
            || ActionText.Search(plugin.Configuration.Language, action).Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private void ExecuteAction(PoseActionEntry action)
    {
        var result = plugin.ExecutionService.Execute(action);
        statusMessage = result.UserMessage;
        statusIsError = !result.Success;
    }

    private void CancelCurrentAction()
    {
        var result = plugin.ExecutionService.CancelCurrentAction();
        statusMessage = result.UserMessage;
        statusIsError = !result.Success;
    }

    private static uint GetTileTopFillColor(ActionExecutionKind executionKind, bool hovered)
        => ImGui.GetColorU32(executionKind == ActionExecutionKind.Timeline
            ? new Vector4(0.19f, 0.21f, 0.25f, hovered ? 0.98f : 0.94f)
            : new Vector4(0.18f, 0.22f, 0.20f, hovered ? 0.98f : 0.94f));

    private static uint GetTileBottomFillColor(ActionExecutionKind executionKind, bool hovered)
        => ImGui.GetColorU32(executionKind == ActionExecutionKind.Timeline
            ? new Vector4(0.12f, 0.13f, 0.15f, hovered ? 0.98f : 0.94f)
            : new Vector4(0.11f, 0.14f, 0.13f, hovered ? 0.98f : 0.94f));

    private static uint GetTileBorderColor(ActionExecutionKind executionKind, bool hovered)
        => ImGui.GetColorU32(executionKind == ActionExecutionKind.Timeline
            ? new Vector4(0.36f, 0.58f, 0.84f, hovered ? 0.85f : 0.50f)
            : new Vector4(0.32f, 0.70f, 0.55f, hovered ? 0.82f : 0.48f));

    private static string BuildTileTitle(string value)
        => WrapForTile(value, TileTitleLineLength, TileTitleMaxLines);

    private static string TrimForTile(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return $"{value[..Math.Max(0, maxLength - 3)]}...";
    }

    private static string WrapForTile(string value, int lineLength, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var lines = new List<string>(maxLines);
        var remaining = value.Trim();

        while (remaining.Length > 0 && lines.Count < maxLines)
        {
            if (remaining.Length <= lineLength)
            {
                lines.Add(remaining);
                break;
            }

            var takeLength = lineLength;
            var breakIndex = remaining.LastIndexOf(' ', Math.Min(lineLength, remaining.Length - 1));
            if (breakIndex > 0)
                takeLength = breakIndex;

            lines.Add(remaining[..takeLength].Trim());
            remaining = remaining[takeLength..].TrimStart();
        }

        if (remaining.Length > 0 && lines.Count > 0)
            lines[^1] = TrimForTile(lines[^1], Math.Max(4, lineLength - 1));

        return string.Join('\n', lines);
    }

    private sealed class ChildRegion : IDisposable
    {
        private readonly string id;
        private readonly Vector2 size;
        private readonly bool border;
        private bool began;

        public ChildRegion(string id, Vector2 size, bool border = false)
        {
            this.id = id;
            this.size = size;
            this.border = border;
        }

        public bool Begin()
        {
            began = true;
            return ImGui.BeginChild(id, size, border);
        }

        public void Dispose()
        {
            if (began)
                ImGui.EndChild();
        }
    }
}
