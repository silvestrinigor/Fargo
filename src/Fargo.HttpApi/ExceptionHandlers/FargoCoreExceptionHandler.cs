using Fargo.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoCoreExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is FargoCoreException coreException)
        {
            ProblemDetails problem = coreException switch
            {
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Core exception."
                },
            };

            problem.Detail = coreException.Message;

            problem.Instance = httpContext.Request.Path;

            problem.Extensions.TryAdd("traceId", httpContext.TraceIdentifier);

            problem.Extensions.TryAdd("coreErrorType", coreException.ErrorType);

            return true;
        }

        return false;
    }
}
