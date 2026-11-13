namespace Fargo.Client.ErrorHandling;

/// <summary>
/// An HTTP error response from the Fargo API, with its status code and parsed problem details.
/// </summary>
public class FargoApiException : Exception
{
    public FargoApiException(
        int statusCode,
        FargoProblemDetails? problem,
        string message)
        : base(message)
    {
        StatusCode = statusCode;
        Problem = problem;
    }

    public int StatusCode { get; }

    public FargoProblemDetails? Problem { get; }
}

/// <summary>
/// Thrown when the API responds with HTTP 401 (Unauthorized).
/// </summary>
public sealed class UnauthorizedApiException : FargoApiException
{
    public UnauthorizedApiException(FargoProblemDetails? problem, string message)
        : base(401, problem, message)
    {
    }
}

/// <summary>
/// Thrown when the API responds with HTTP 403 (Forbidden).
/// </summary>
public sealed class ForbiddenApiException : FargoApiException
{
    public ForbiddenApiException(FargoProblemDetails? problem, string message)
        : base(403, problem, message)
    {
    }
}

/// <summary>
/// Thrown when the API responds with HTTP 400 (Bad Request). Field-level validation errors are
/// available from <see cref="FargoProblemDetails.GetValidationErrors"/>.
/// </summary>
public sealed class BadRequestApiException : FargoApiException
{
    public BadRequestApiException(FargoProblemDetails? problem, string message)
        : base(400, problem, message)
    {
    }
}
