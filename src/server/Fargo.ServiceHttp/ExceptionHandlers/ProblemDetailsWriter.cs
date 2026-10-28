using Fargo.Core.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.ServiceHttp.ExceptionHandlers;

/// <summary>
/// Shared helpers for the Fargo exception handlers. Centralizes the common
/// problem-details tail (<c>Instance</c>, <c>traceId</c>, <c>errorType</c>) and
/// the final <see cref="IProblemDetailsService.WriteAsync"/> call so each
/// handler only has to describe what is unique about its exception.
/// </summary>
internal static class ProblemDetailsWriter
{
    /// <summary>
    /// Writes <paramref name="problem"/> for the given exception, stamping the
    /// request path and trace identifier and, when the exception carries a
    /// <see cref="FargoErrorType"/>, the error type.
    /// </summary>
    public static ValueTask WriteAsync(
        IProblemDetailsService problemDetailsService,
        HttpContext httpContext,
        Exception exception,
        ProblemDetails problem,
        FargoErrorType? errorType = null)
    {
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (errorType is not null)
        {
            problem.Extensions["errorType"] = errorType;
        }

        return problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }
}
