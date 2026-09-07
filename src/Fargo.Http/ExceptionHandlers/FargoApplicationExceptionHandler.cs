using Fargo.Application.Common;
using Fargo.Application.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.Http.ExceptionHandlers;

/// <summary>
/// Represents an exception handler for Fargo application exceptions that converts them into appropriate HTTP problem details responses.
/// This handler specifically processes Fargo application exceptions and maps them to standard HTTP status codes with detailed error information.
/// </summary>
/// <param name="problemDetailsService">The service used to write problem details responses</param>
public sealed class FargoApplicationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the specified exception by converting it into an appropriate HTTP response with problem details.
    /// This method processes Fargo application exceptions and returns true if the exception was handled, false otherwise.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request</param>
    /// <param name="exception">The exception to handle</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A value task that represents the asynchronous operation, with true if the exception was handled, false otherwise</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FargoApplicationException appException)
        {
            return false;
        }

        ProblemDetails problem;

        switch (appException)
        {
            case ActorAccessDeniedFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Access denied.",
                    Detail = ex.Message,
                };

                problem.Extensions["actorGuid"] = ex.ActorGuid;
                problem.Extensions["actorType"] = ex.ActorType;
                problem.Extensions["entityGuid"] = ex.EntityGuid;
                problem.Extensions["entityType"] = ex.EntityType;
                break;

            case ActorPermissionDeniedFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Permission denied.",
                    Detail = ex.Message,
                };

                problem.Extensions["actorId"] = ex.ActorGuid;
                problem.Extensions["actorType"] = ex.ActorType;
                problem.Extensions["actionType"] = ex.ActionType;
                break;

            case EntityNotFoundFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Entity not found.",
                    Detail = ex.Message,
                };

                problem.Extensions["entityGuid"] = ex.EntityGuid;
                problem.Extensions["entityType"] = ex.EntityType;
                break;

            case ActorNotFoundFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Actor not found.",
                    Detail = ex.Message,
                };

                problem.Extensions["actorGuid"] = ex.ActorGuid;
                problem.Extensions["actorType"] = ex.ActorType;

                break;

            case InvalidCredentialsFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials.",
                    Detail = ex.Message,
                };

                break;

            case UserPasswordChangeRequiredFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Password change required.",
                    Detail = ex.Message,
                };

                problem.Extensions["userGuid"] = ex.UserGuid;

                break;

            case AuthenticationRateLimitFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many authentication attempts.",
                    Detail = ex.Message
                };

                problem.Extensions["retryAfter"] = ex.RetryAfter;

                break;

            default:

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Application error.",
                    Detail = appException.Message,
                };

                break;
        }

        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["errorType"] = appException.ErrorType;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });

        return true;
    }
}
