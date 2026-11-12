using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Exceptions.Identity;
using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.ServiceHttp.ExceptionHandlers;

public sealed class FargoApplicationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
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

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials.",
                    Detail = ex.Message,
                };

                break;

            case UserPasswordChangeRequiredFargoApplicationException ex:

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Password change required.",
                    Detail = ex.Message,
                };

                problem.Extensions["userGuid"] = ex.UserGuid;

                break;

            case AuthenticationRateLimitFargoApplicationException ex:

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many authentication attempts.",
                    Detail = ex.Message
                };

                problem.Extensions["retryAfter"] = ex.RetryAfter;

                break;

            case IdempotencyKeyReusedFargoApplicationException ex:

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Idempotency key reused.",
                    Detail = ex.Message,
                };

                break;

            default:

                problem = new ProblemDetails
                {
                    Status = appException.ErrorType == FargoErrorType.Conflict
                        ? StatusCodes.Status409Conflict
                        : StatusCodes.Status400BadRequest,
                    Title = appException.ErrorType == FargoErrorType.Conflict
                        ? "Conflict."
                        : "Application error.",
                    Detail = appException.Message,
                };

                break;
        }

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, appException.ErrorType);

        return true;
    }
}
