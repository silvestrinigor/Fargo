using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.ServiceHttp.ExceptionHandlers;

public sealed class BadRequestExceptionHandler(
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid request",
            Detail = badRequest.InnerException?.Message ?? badRequest.Message,
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem);

        return true;
    }
}
