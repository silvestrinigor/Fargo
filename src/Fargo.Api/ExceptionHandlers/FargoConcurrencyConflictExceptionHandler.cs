using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Api.ExceptionHandlers;

/// <summary>
/// Turns a <see cref="DbUpdateConcurrencyException"/> (the <c>xmin</c> concurrency token no longer matches)
/// into a 409 Conflict instead of a 500, because it is recoverable: re-read and retry. Registered ahead of
/// the generic Fargo handlers so it matches first.
/// </summary>
public sealed class FargoConcurrencyConflictExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateConcurrencyException)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Concurrency conflict.",
            Detail = "The resource was modified by another request. " +
                     "Re-read the current state and retry the operation.",
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, FargoErrorType.Conflict);

        return true;
    }
}
