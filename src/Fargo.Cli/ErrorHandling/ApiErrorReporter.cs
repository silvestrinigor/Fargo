using Fargo.ClientHttp.ErrorHandling;
using System.Net.Sockets;

namespace Fargo.Cli.ErrorHandling;

/// <summary>
/// Renders Fargo API errors as concise, user-friendly console messages and maps
/// them to process exit codes. Centralizes the handling of API error responses
/// (400 with validation errors, 401, 403, and other non-success statuses) and
/// connectivity failures so every command surfaces the same clear output
/// instead of raw stack traces.
/// </summary>
public static class ApiErrorReporter
{
    /// <summary>
    /// When enabled (via the <c>--verbose</c> global flag), error output also
    /// includes technical diagnostics: the exception type, inner exceptions,
    /// and stack trace. Off by default so normal output stays concise.
    /// </summary>
    public static bool Verbose { get; set; }

    /// <summary>
    /// Writes a message for <paramref name="exception"/> to standard error and
    /// returns the exit code the process should use.
    /// </summary>
    public static int Report(Exception exception)
    {
        switch (exception)
        {
            case UnauthorizedApiException unauthorized:
                WriteProblem(unauthorized);
                WriteVerboseDetails(unauthorized);
                return 1;

            case ForbiddenApiException forbidden:
                WriteProblem(forbidden);
                WriteVerboseDetails(forbidden);
                return 1;

            case BadRequestApiException badRequest:
                WriteProblem(badRequest);
                WriteValidationErrors(badRequest);
                WriteVerboseDetails(badRequest);
                return 1;

            case FargoApiException api:
                Console.Error.WriteLine(api.Message);
                WriteVerboseDetails(api);
                return 1;

            default:
                if (IsConnectivityError(exception))
                {
                    WriteConnectivityError(exception);
                    WriteVerboseDetails(exception);
                    return 1;
                }

                // Not an error we specialize in; let the caller decide.
                throw exception;
        }
    }

    /// <summary>
    /// Pure predicate (no output) that reports whether <paramref name="exception"/>
    /// is a user-facing error this reporter knows how to render. Safe to use in
    /// an exception filter (<c>catch ... when</c>); the actual rendering should
    /// happen in the catch body via <see cref="Report"/>.
    /// </summary>
    public static bool CanReport(Exception exception) =>
        exception is FargoApiException || IsConnectivityError(exception);

    /// <summary>
    /// Attempts to report <paramref name="exception"/> as a known, user-facing
    /// error (a Fargo API error or a connectivity failure). Returns
    /// <c>true</c> and sets <paramref name="exitCode"/> when handled; returns
    /// <c>false</c> for exceptions that should propagate.
    /// </summary>
    public static bool TryReport(Exception exception, out int exitCode)
    {
        if (!CanReport(exception))
        {
            exitCode = 0;
            return false;
        }

        exitCode = Report(exception);
        return true;
    }

    /// <summary>
    /// Determines whether <paramref name="exception"/> represents a failure to
    /// reach the server (connection refused, DNS failure, TLS handshake error,
    /// request timeout, dropped connection, ...) rather than an HTTP error
    /// response from the API.
    /// </summary>
    private static bool IsConnectivityError(Exception exception)
    {
        // Unwrap to inspect inner exceptions too: the HTTP/timeout stack often
        // wraps a SocketException inside an HttpRequestException, and Kiota may
        // surface a TaskCanceledException for timeouts.
        for (var current = exception; current is not null; current = current.InnerException)
        {
            switch (current)
            {
                case HttpRequestException:
                case SocketException:
                case System.IO.IOException:
                // A request timeout surfaces as a cancellation that was not
                // requested by the user.
                case TaskCanceledException:
                case OperationCanceledException:
                    return true;
            }
        }

        return false;
    }

    private static void WriteConnectivityError(Exception exception)
    {
        Console.Error.WriteLine(
            "Could not connect to the Fargo server. Make sure the server is "
            + "running and the configured URL is correct (check "
            + "'fargo config show' or pass --server <url>).");
    }

    /// <summary>
    /// Writes field-level validation errors (field: message) for a bad-request
    /// response, when the problem details carry an <c>errors</c> map. Does
    /// nothing when there are none.
    /// </summary>
    private static void WriteValidationErrors(FargoApiException exception)
    {
        var problem = exception.Problem;

        if (problem is null)
        {
            return;
        }

        var errors = problem.GetValidationErrors();

        if (errors.Count == 0)
        {
            return;
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine("Validation errors:");

        foreach (var (field, messages) in errors)
        {
            foreach (var message in messages)
            {
                Console.Error.WriteLine($"  - {field}: {message}");
            }
        }
    }

    /// <summary>
    /// When <see cref="Verbose"/> is enabled, writes technical diagnostics for
    /// <paramref name="exception"/> (type, status code, inner exceptions and
    /// stack trace) to standard error, below the user-facing message.
    /// </summary>
    private static void WriteVerboseDetails(Exception exception)
    {
        if (!Verbose)
        {
            return;
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine("--- Verbose diagnostics ---");

        if (exception is FargoApiException api)
        {
            Console.Error.WriteLine($"HTTP status: {api.StatusCode}");
        }

        for (var current = exception; current is not null; current = current.InnerException)
        {
            Console.Error.WriteLine($"{current.GetType().FullName}: {current.Message}");
        }

        if (exception.StackTrace is not null)
        {
            Console.Error.WriteLine(exception.StackTrace);
        }
    }

    private static void WriteProblem(FargoApiException exception)
    {
        Console.Error.WriteLine(exception.Message);

        var problem = exception.Problem;

        if (problem is null)
        {
            return;
        }

        // Surface the correlation id when the server provides one, which helps
        // when reporting issues.
        var traceId = problem.GetExtensionString("traceId");

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            Console.Error.WriteLine($"Trace ID: {traceId}");
        }
    }
}
