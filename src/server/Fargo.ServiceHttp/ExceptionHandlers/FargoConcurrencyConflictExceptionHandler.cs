using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fargo.ServiceHttp.ExceptionHandlers;

/// <summary>
/// Translates an EF Core <see cref="DbUpdateConcurrencyException"/> — raised when
/// a tracked aggregate's concurrency token (the PostgreSQL <c>xmin</c>) no longer
/// matches the row at save time — into an RFC 7807 <c>409 Conflict</c> response.
///
/// Without this handler the exception would fall through to the default
/// framework handler and surface as a 500, masking what is really a recoverable
/// "the resource changed under you; re-read and retry" condition.
///
/// Registered ahead of the generic Fargo handlers so the concurrency case is
/// matched before any broader database-exception handling.
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
