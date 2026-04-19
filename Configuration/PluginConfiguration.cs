using Dalamud.Configuration;
using PosePad.Localization;

namespace PosePad.Configuration;

[Serializable]
public sealed class PluginConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public WindowState MainWindow { get; set; } = new()
    {
        IsOpen = true,
        Width = 1040,
        Height = 680,
        HasSize = true,
    };

    public WindowState ConfigWindow { get; set; } = new()
    {
        IsOpen = false,
        Width = 420,
        Height = 240,
        HasSize = true,
    };

    public HashSet<string> FavoriteActionIds { get; set; } = [];
    public List<string> RecentActionIds { get; set; } = [];
    public UiLanguage Language { get; set; } = UiLanguage.English;
    public bool DisableGposeUiHide { get; set; } = true;
    public bool OpenOnEnterGPose { get; set; }

    public void Save()
        => Plugin.PluginInterface.SavePluginConfig(this);

    public bool IsFavorite(string actionId)
        => FavoriteActionIds.Contains(actionId);

    public bool SetFavorite(string actionId, bool isFavorite)
    {
        return isFavorite
            ? FavoriteActionIds.Add(actionId)
            : FavoriteActionIds.Remove(actionId);
    }

    public void PushRecent(string actionId)
    {
        RecentActionIds.Remove(actionId);
        RecentActionIds.Insert(0, actionId);

        if (RecentActionIds.Count > 15)
            RecentActionIds.RemoveRange(15, RecentActionIds.Count - 15);
    }

    public void ClearRecent()
        => RecentActionIds.Clear();
}
