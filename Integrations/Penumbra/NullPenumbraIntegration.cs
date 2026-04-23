namespace PosePad.Integrations.Penumbra;

public sealed class NullPenumbraIntegration : IPenumbraIntegration
{
    public bool IsAvailable => false;

    public string StatusMessage
        => "V1 does not include automatic Penumbra action-mod detection. This is an extension point only.";

    public IReadOnlyList<Models.PoseActionEntry> GetModActions()
        => [];
}
