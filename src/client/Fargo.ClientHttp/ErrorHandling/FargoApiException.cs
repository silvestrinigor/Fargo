namespace Fargo.ClientHttp.ErrorHandling;

/// <summary>
/// Base exception for HTTP error responses returned by the Fargo API. Carries
/// the HTTP status code and, when available, the parsed problem details so
/// callers can present a meaningful message.
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
/// Thrown when the API responds with HTTP 401 (Unauthorized), for example when
/// the access token is missing, expired, or the credentials are invalid.
/// </summary>
public sealed class UnauthorizedApiException : FargoApiException
{
    public UnauthorizedApiException(FargoProblemDetails? problem, string message)
        : base(401, problem, message)
    {
    }
}

/// <summary>
/// Thrown when the API responds with HTTP 403 (Forbidden), for example when the
/// authenticated actor lacks permission for the requested action.
/// </summary>
public sealed class ForbiddenApiException : FargoApiException
{
    public ForbiddenApiException(FargoProblemDetails? problem, string message)
        : base(403, problem, message)
    {
    }
}

/// <summary>
/// Thrown when the API responds with HTTP 400 (Bad Request), for example when a
/// request fails model validation or an application rule rejects the input. The
/// <see cref="FargoApiException.Problem"/> may carry field-level validation
/// errors (see <see cref="FargoProblemDetails.GetValidationErrors"/>).
/// </summary>
public sealed class BadRequestApiException : FargoApiException
{
    public BadRequestApiException(FargoProblemDetails? problem, string message)
        : base(400, problem, message)
    {
    }
}
