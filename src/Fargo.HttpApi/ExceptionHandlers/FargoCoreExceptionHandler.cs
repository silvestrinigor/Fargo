using Fargo.Core.Common;
using Fargo.Core.Shared.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.ExceptionHandlers;

public sealed class FargoCoreExceptionHandler(
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FargoCoreException coreException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Core error.",
            Detail = coreException.Message,
            Instance = httpContext.Request.Path,
        };

        if (coreException.ErrorType == FargoErrorType.InvalidOperation)
        {
            problem.Title = "Core invalid operation.";
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["errorType"] = coreException.ErrorType;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });

        return true;
    }
}
