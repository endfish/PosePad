using PosePad.Configuration;
using Dalamud.Bindings.ImGui;

namespace PosePad.Windows;

internal sealed class WindowStateTracker
{
    private const long SaveIntervalMilliseconds = 1000;

    private readonly PluginConfiguration configuration;
    private long lastSaveTick;
    private bool dirty;

    public WindowStateTracker(PluginConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Capture(WindowState state, bool isOpen)
    {
        var position = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();

        dirty |= state.IsOpen != isOpen;
        dirty |= !state.HasPosition || state.PositionX != position.X || state.PositionY != position.Y;
        dirty |= !state.HasSize || state.Width != size.X || state.Height != size.Y;

        state.IsOpen = isOpen;
        state.HasPosition = true;
        state.PositionX = position.X;
        state.PositionY = position.Y;
        state.HasSize = true;
        state.Width = size.X;
        state.Height = size.Y;

        if (!dirty)
            return;

        var now = Environment.TickCount64;
        if (now - lastSaveTick < SaveIntervalMilliseconds)
            return;

        configuration.Save();
        lastSaveTick = now;
        dirty = false;
    }

    public void ForceSave(WindowState state, bool isOpen)
    {
        state.IsOpen = isOpen;
        configuration.Save();
        lastSaveTick = Environment.TickCount64;
        dirty = false;
    }
}
