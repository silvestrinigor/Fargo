using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Fargo.ServiceHttp.ExceptionHandlers;

/// <summary>
/// Turns a unique-constraint violation (SQLSTATE <c>23505</c>) into a 409 Conflict instead of a 500.
/// The application pre-checks duplicates (for example EAN-13 availability), but two concurrent creates can
/// both pass that check; the unique index is the authoritative guard. Registered ahead of the generic Fargo
/// handlers so it matches first.
/// </summary>
public sealed class FargoUniqueConstraintExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private const string UniqueViolationSqlState = "23505";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException || !IsUniqueViolation(exception))
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict.",
            Detail = "The operation conflicts with an existing resource. A value " +
                     "that must be unique is already in use.",
        };

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService, httpContext, exception, problem, FargoErrorType.Conflict);

        return true;
    }

    // Reading SqlState through the provider-agnostic DbException keeps this handler free of a direct Npgsql
    // dependency.
    private static bool IsUniqueViolation(Exception exception)
    {
        for (var current = exception.InnerException; current is not null; current = current.InnerException)
        {
            if (current is DbException dbException &&
                string.Equals(dbException.SqlState, UniqueViolationSqlState, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
