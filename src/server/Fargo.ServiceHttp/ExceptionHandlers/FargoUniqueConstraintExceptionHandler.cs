using System.Data.Common;
using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fargo.ServiceHttp.ExceptionHandlers;

/// <summary>
/// Translates a database unique-constraint violation (PostgreSQL SQLSTATE
/// <c>23505</c>) — surfaced by EF Core as a <see cref="DbUpdateException"/>
/// wrapping a provider <see cref="DbException"/> — into an RFC 7807
/// <c>409 Conflict</c> response.
///
/// The application performs a best-effort pre-check for duplicates (for example
/// the EAN-13 availability check), but that check has a time-of-check/
/// time-of-use race: two concurrent creates can both pass it and then both
/// insert. The unique index is the authoritative guard and rejects the second
/// insert; this handler turns that low-level failure into a clean, typed 409
/// instead of letting it fall through to a generic 500.
///
/// Registered ahead of the generic Fargo handlers so the conflict is matched
/// before any broader database-exception handling.
/// </summary>
public sealed class FargoUniqueConstraintExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    // PostgreSQL unique_violation. See https://www.postgresql.org/docs/current/errcodes-appendix.html
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

    // Walks the inner-exception chain looking for a provider DbException whose
    // SQLSTATE is the unique-violation code. Reading SqlState through the
    // provider-agnostic System.Data.Common.DbException keeps this handler free of
    // a direct Npgsql dependency.
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