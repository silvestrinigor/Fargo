using Fargo.Core.Common;
using Fargo.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.Http.ExceptionHandlers;

public sealed class FargoInfrastructureExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not FargoInfrastructureException infraException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Infra error.",
            Detail = infraException.Message,
            Instance = httpContext.Request.Path,
        };

        if (infraException.ErrorType == FargoErrorType.InvalidOperation)
        {
            problem.Title = "Infra invalid operation.";
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["errorType"] = infraException.ErrorType;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });

        return true;
    }
}
