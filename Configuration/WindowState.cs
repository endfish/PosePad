namespace PosePad.Configuration;

[Serializable]
public sealed class WindowState
{
    public bool IsOpen { get; set; }

    public bool HasPosition { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public bool HasSize { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
