namespace Fargo.Api;

/// <summary>
/// Base class for exceptions thrown when the API returns a domain error.
/// </summary>
public class FargoSdkApiException : FargoSdkException
{
    internal FargoSdkApiException(string detail)
        : base(detail)
    {
    }

    internal FargoSdkApiException(FargoSdkError error)
        : base(CreateMessage(error))
    {
        ErrorType = error.Type;
        Title = error.Title;
        ProblemType = error.ProblemType;
        StatusCode = error.StatusCode;
        Instance = error.Instance;
        TraceId = error.TraceId;
    }

    /// <summary>Gets the SDK error category associated with this API failure.</summary>
    public FargoSdkErrorType ErrorType { get; }

    /// <summary>Gets the short problem title returned by the backend, when available.</summary>
    public string? Title { get; }

    /// <summary>Gets the machine-readable backend problem type, when available.</summary>
    public string? ProblemType { get; }

    /// <summary>Gets the HTTP status code returned by the backend, when available.</summary>
    public int? StatusCode { get; }

    /// <summary>Gets the backend request instance/path, when available.</summary>
    public string? Instance { get; }

    /// <summary>Gets the backend trace identifier, when available.</summary>
    public string? TraceId { get; }

    private static string CreateMessage(FargoSdkError error)
    {
        if (!string.IsNullOrWhiteSpace(error.Detail)
            && !string.Equals(error.Detail, "An unexpected error occurred.", StringComparison.Ordinal))
        {
            return error.Detail;
        }

        var summary = error.Title ?? error.Detail ?? "An unexpected error occurred.";

        if (!string.IsNullOrWhiteSpace(error.TraceId))
        {
            return $"{summary} TraceId: {error.TraceId}";
        }

        if (!string.IsNullOrWhiteSpace(error.ProblemType))
        {
            return $"{summary} ({error.ProblemType})";
        }

        return summary;
    }
}
