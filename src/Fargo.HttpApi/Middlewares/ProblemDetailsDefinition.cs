namespace Fargo.HttpApi.Middlewares;

/// <summary>
/// Represents the metadata used to build a ProblemDetails response.
/// </summary>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Title">The problem title.</param>
/// <param name="Type">The machine-readable problem type.</param>
public sealed record ProblemDetailsDefinition(
        int StatusCode,
        string Title,
        string Type
        );