using Fargo.Application;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoApplicationExceptionHandler(
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
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
            case AccessDeniedFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Access denied.",
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path,
                };

                problem.Extensions["actorGuid"] = ex.ActorGuid;
                problem.Extensions["actorType"] = ex.ActorType;
                problem.Extensions["entityGuid"] = ex.EntityGuid;
                problem.Extensions["entityType"] = ex.EntityType;
                break;

            case PermissionDeniedFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Permission denied.",
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path,
                };

                problem.Extensions["actorId"] = ex.ActorGuid;
                problem.Extensions["actionType"] = ex.ActionType;
                break;

            case EntityNotFoundFargoApplicationException ex:

                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Entity not found.",
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path,
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
                    Instance = httpContext.Request.Path,
                };

                problem.Extensions["actorGuid"] = ex.ActorGuid;
                problem.Extensions["actorType"] = ex.ActorType;

                break;

            default:

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Application error.",
                    Detail = appException.Message,
                    Instance = httpContext.Request.Path,
                };

                break;
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["appErrorType"] = appException.ErrorType;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });

        return true;
    }
}

