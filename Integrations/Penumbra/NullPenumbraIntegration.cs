namespace PosePad.Integrations.Penumbra;

public sealed class NullPenumbraIntegration : IPenumbraIntegration
{
    public bool IsAvailable => false;

    public string StatusMessage
        => "Penumbra integration is unavailable.";

    public IReadOnlyList<Models.PoseActionEntry> GetModActions()
        => [];
}
