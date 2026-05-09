using Fargo.Sdk.Contracts.Errors;

namespace Fargo.Sdk.Contracts;

/// <summary>Represents the result of a Fargo API operation that returns a response body.</summary>
/// <typeparam name="TResult">The success result type.</typeparam>
public sealed record FargoResponse<TResult>(
    TResult? Result,
    FargoProblemDetails? Error,
    int StatusCode)
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess => Error is null;
}

/// <summary>Represents the result of a Fargo API operation that does not return a response body.</summary>
public sealed record FargoResponse(
    FargoProblemDetails? Error,
    int StatusCode)
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess => Error is null;
}
