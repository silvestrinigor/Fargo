using Fargo.Core.Common;
using Fargo.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.ServiceHttp.ExceptionHandlers;

public sealed class FargoInfrastructureExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not FargoInfrastructureException infraException)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = infraException.ErrorType == FargoErrorType.InvalidOperation
                ? "Infra invalid operation."
                : "Infra error.",
            Detail = infraException.Message,
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, infraException.ErrorType);

        return true;
    }
}
