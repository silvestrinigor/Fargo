using Fargo.Application;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoApplicationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FargoApplicationException appException)
        {
            ProblemDetails problem;

            switch (appException)
            {
                case AccessDeniedFargoApplicationException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access denied.",
                    };

                    problem.Extensions.Add("actorId", ex.ActorId);

                    problem.Extensions.Add("entityGuid", ex.EntityGuid);

                    problem.Extensions.Add("entityType", ex.EntityType);

                    break;

                case PermissionDeniedFargoApplicationException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Permission denied.",
                    };

                    problem.Extensions.Add("actorId", ex.ActorId);

                    problem.Extensions.Add("actionType", ex.ActionType);

                    break;

                case EntityNotFoundFargoApplicationException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Entity not found.",
                    };

                    problem.Extensions.Add("entityGuid", ex.EntityGuid);

                    problem.Extensions.Add("entityType", ex.EntityType);

                    break;

                case ActorNotFoundFargoApplicationException ex:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Actor not found.",
                    };

                    problem.Extensions.Add("actorId", ex.ActorId);

                    break;

                default:

                    problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Core exception."
                    };

                    break;
            }

            problem.Detail = appException.Message;

            problem.Instance = httpContext.Request.Path;

            problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

            problem.Extensions.TryAdd("appErrorType", appException.ErrorType);

            return true;
        }

        return false;
    }
}
