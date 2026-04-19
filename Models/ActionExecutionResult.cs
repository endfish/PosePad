namespace PosePad.Models;

public sealed record ActionExecutionResult(
    bool Success,
    string UserMessage);
