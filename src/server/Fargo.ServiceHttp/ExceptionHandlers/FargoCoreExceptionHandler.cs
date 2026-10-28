using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.ServiceHttp.ExceptionHandlers;

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

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = coreException.ErrorType == FargoErrorType.InvalidOperation
                ? "Core invalid operation."
                : "Core error.",
            Detail = coreException.Message,
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, coreException.ErrorType);

        return true;
    }
}
