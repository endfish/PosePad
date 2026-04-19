using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PosePad.Localization;

namespace PosePad.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly WindowStateTracker stateTracker;

    public ConfigWindow(Plugin plugin)
        : base($"{UiText.SettingsWindowTitle(plugin.Configuration.Language)}###PosePadConfig")
    {
        this.plugin = plugin;
        stateTracker = new WindowStateTracker(plugin.Configuration);
        IsOpen = plugin.Configuration.ConfigWindow.IsOpen;

        if (plugin.Configuration.ConfigWindow.HasSize)
        {
            Size = new Vector2(plugin.Configuration.ConfigWindow.Width, plugin.Configuration.ConfigWindow.Height);
            SizeCondition = ImGuiCond.FirstUseEver;
        }
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var language = plugin.Configuration.Language;
        WindowName = $"{UiText.SettingsWindowTitle(language)}###PosePadConfig";

        var selectedLanguage = plugin.Configuration.Language;
        if (ImGui.BeginCombo(UiText.Language(language), UiText.LanguageOption(selectedLanguage)))
        {
            foreach (var option in Enum.GetValues<UiLanguage>())
            {
                var isSelected = option == selectedLanguage;
                if (ImGui.Selectable(UiText.LanguageOption(option), isSelected))
                {
                    plugin.Configuration.Language = option;
                    plugin.Configuration.Save();
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Text(UiText.FavoriteCount(language, plugin.Configuration.FavoriteActionIds.Count));
        ImGui.Text(UiText.RecentCount(language, plugin.Configuration.RecentActionIds.Count));
        ImGui.Spacing();

        var disableGposeUiHide = plugin.Configuration.DisableGposeUiHide;
        if (ImGui.Checkbox(UiText.DisableGposeUiHide(language), ref disableGposeUiHide))
        {
            plugin.Configuration.DisableGposeUiHide = disableGposeUiHide;
            plugin.ApplyUiVisibilitySettings();
            plugin.Configuration.Save();
        }

        var openOnEnterGPose = plugin.Configuration.OpenOnEnterGPose;
        if (ImGui.Checkbox(UiText.OpenOnEnterGPose(language), ref openOnEnterGPose))
        {
            plugin.Configuration.OpenOnEnterGPose = openOnEnterGPose;
            plugin.Configuration.Save();
        }

        if (ImGui.Button(UiText.ClearRecent(language)))
        {
            plugin.Configuration.ClearRecent();
            plugin.Configuration.Save();
        }

        stateTracker.Capture(plugin.Configuration.ConfigWindow, IsOpen);
    }

    public override void OnClose()
    {
        plugin.Configuration.ConfigWindow.IsOpen = false;
        stateTracker.ForceSave(plugin.Configuration.ConfigWindow, false);
        base.OnClose();
    }
}
