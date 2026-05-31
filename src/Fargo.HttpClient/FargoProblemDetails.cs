namespace Fargo.HttpClient;

public sealed record FargoProblemDetailsDto
{
    public int Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Instance { get; init; } = string.Empty;

    public string TraceId { get; init; } = string.Empty;
}
