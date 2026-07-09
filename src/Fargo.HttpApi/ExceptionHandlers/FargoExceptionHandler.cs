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
                case AccessDeniedFargoException ex:

                    problem = new ProblemDetails
                    {
                        Type = "core/actors/access-denied",
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access denied.",
                        Detail = ex.Message
                    };

                    problem.Extensions.Add("actorId", ex.ActorId);

                    problem.Extensions.Add("entityGuid", ex.EntityGuid);

                    problem.Extensions.Add("entityType", ex.EntityType);

                    break;
            }
        }

        return false;
    }
}
