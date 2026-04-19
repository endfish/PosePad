namespace PosePad.Integrations.Penumbra;

public interface IPenumbraIntegration
{
    bool IsAvailable { get; }
    string StatusMessage { get; }
}
