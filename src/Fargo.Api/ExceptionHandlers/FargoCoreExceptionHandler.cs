using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.Api.ExceptionHandlers;

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
            Status = coreException.ErrorType == FargoErrorType.Conflict
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest,
            Title = coreException.ErrorType switch
            {
                FargoErrorType.Conflict => "Conflict.",
                FargoErrorType.InvalidOperation => "Core invalid operation.",
                _ => "Core error.",
            },
            Detail = coreException.Message,
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, coreException.ErrorType);

        return true;
    }
}
