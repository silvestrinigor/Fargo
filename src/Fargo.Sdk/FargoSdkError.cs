namespace Fargo.Api;

/// <summary>
/// Describes a failure returned by the Fargo API.
/// </summary>
public sealed class FargoSdkError
{
    internal FargoSdkError(
        FargoSdkErrorType type,
        string detail,
        string? title = null,
        string? problemType = null,
        int? statusCode = null,
        string? instance = null,
        string? traceId = null)
    {
        Type = type;
        Detail = detail;
        Title = title;
        ProblemType = problemType;
        StatusCode = statusCode;
        Instance = instance;
        TraceId = traceId;
    }

    /// <summary>Gets the category of the error.</summary>
    public FargoSdkErrorType Type { get; }

    /// <summary>Gets a human-readable description of the error.</summary>
    public string Detail { get; }

    /// <summary>Gets a short human-readable summary of the error.</summary>
    public string? Title { get; }

    /// <summary>Gets the machine-readable problem type returned by the backend.</summary>
    public string? ProblemType { get; }

    /// <summary>Gets the HTTP status code returned by the backend.</summary>
    public int? StatusCode { get; }

    /// <summary>Gets the backend instance/path associated with the error, when provided.</summary>
    public string? Instance { get; }

    /// <summary>Gets the backend trace identifier associated with the error, when provided.</summary>
    public string? TraceId { get; }
}
