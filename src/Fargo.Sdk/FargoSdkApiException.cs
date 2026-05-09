using Fargo.Sdk.Contracts.Errors;
using System.Net;

namespace Fargo.Sdk;

/// <summary>
/// Base class for exceptions thrown when the API returns a domain error.
/// </summary>
public class FargoSdkApiException : FargoSdkException
{
    internal FargoSdkApiException(string detail)
        : base(detail)
    {
    }

    internal FargoSdkApiException(FargoProblemDetails problem, HttpStatusCode statusCode)
        : base(CreateMessage(problem))
    {
        Problem = problem;
        Title = problem.Title;
        ProblemType = problem.Type;
        StatusCode = problem.Status ?? (int)statusCode;
        Instance = problem.Instance;
        TraceId = problem.TraceId;
    }

    /// <summary>Gets the problem details returned by the backend.</summary>
    public FargoProblemDetails? Problem { get; }

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

    private static string CreateMessage(FargoProblemDetails problem)
    {
        if (!string.IsNullOrWhiteSpace(problem.Detail)
            && !string.Equals(problem.Detail, "An unexpected error occurred.", StringComparison.Ordinal))
        {
            return problem.Detail;
        }

        var summary = problem.Title ?? problem.Detail ?? "An unexpected error occurred.";

        if (!string.IsNullOrWhiteSpace(problem.TraceId))
        {
            return $"{summary} TraceId: {problem.TraceId}";
        }

        if (!string.IsNullOrWhiteSpace(problem.Type))
        {
            return $"{summary} ({problem.Type})";
        }

        return summary;
    }
}
