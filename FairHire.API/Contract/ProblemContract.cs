namespace FairHire.API.Contract;

/// <summary>
/// Мінімальний контракт ProblemDetails, 
/// з додатковими полями TraceId/RequestId.
/// </summary>

public sealed class ProblemContract
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int? Status { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }


    // додаткові поля для кореляції/трасування
    public string? TraceId { get; init; }
    public string? RequestId { get; init; }
}
