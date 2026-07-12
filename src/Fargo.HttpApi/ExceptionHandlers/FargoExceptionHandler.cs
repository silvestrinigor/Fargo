using Fargo.Application;
using Fargo.Core;
using Fargo.Core.Actors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FargoException fargoException)
        {
            ProblemDetails problem;

            switch (fargoException)
            {
                #region  Core

                case AccessDeniedFargoException ex:

                    problem = new ProblemDetails
                    {
                        Type = "actor/access-denied",
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access denied.",
                        Detail = ex.Message,
                        Instance = httpContext.Request.Path
                    };

                    problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

                    problem.Extensions.Add("actorId", ex.ActorId);

                    problem.Extensions.Add("entityGuid", ex.EntityGuid);

                    problem.Extensions.Add("entityType", ex.EntityType);

                    break;

                case PermissionDeniedFargoException ex:

                    problem = new ProblemDetails
                    {
                        Type = "actor/permission-denied",
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Permission denied.",
                        Detail = ex.Message,
                        Instance = httpContext.Request.Path
                    };

                    problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

                    problem.Extensions.Add("actorId", ex.ActorId);

                    problem.Extensions.Add("actionType", ex.ActionType);

                    break;

                #endregion

                #region  Application

                case EntityNotFoundFargoException ex:

                    problem = new ProblemDetails
                    {
                        Type = "entity/not-found",
                        Status = StatusCodes.Status404NotFound,
                        Title = "Entity not found.",
                        Detail = ex.Message,
                        Instance = httpContext.Request.Path
                    };

                    problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

                    problem.Extensions.Add("entityGuid", ex.EntityGuid);

                    problem.Extensions.Add("entityType", ex.EntityType);

                    break;

                case ActorNotFoundFargoException ex:

                    problem = new ProblemDetails
                    {
                        Type = "actor/not-found",
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Actor not found.",
                        Detail = ex.Message,
                        Instance = httpContext.Request.Path
                    };

                    problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

                    problem.Extensions.Add("actorId", ex.ActorId);

                    break;

                    #endregion
            }
        }

        return false;
    }
}
