namespace Fargo.Sdk.Http;

/// <summary>
/// Represents an RFC 7807 problem details response from the API.
/// </summary>
public sealed class FargoProblemDetails
{
    public string? Type { get; init; }

    public string? Title { get; init; }

    public string? Detail { get; init; }

    public int? Status { get; init; }

    public string? Instance { get; init; }
}
