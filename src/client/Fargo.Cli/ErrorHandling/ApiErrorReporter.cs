using Fargo.ClientHttp.ErrorHandling;
using System.Net.Sockets;

namespace Fargo.Cli.ErrorHandling;

/// <summary>
/// Renders Fargo API errors as concise, user-friendly console messages and maps
/// them to process exit codes. Centralizes the handling of 401/403 responses
/// and connectivity failures so every command surfaces the same clear output
/// instead of raw stack traces.
/// </summary>
public static class ApiErrorReporter
{
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
                return 1;

            case ForbiddenApiException forbidden:
                WriteProblem(forbidden);
                return 1;

            case FargoApiException api:
                Console.Error.WriteLine(api.Message);
                return 1;

            default:
                if (IsConnectivityError(exception))
                {
                    WriteConnectivityError(exception);
                    return 1;
                }

                // Not an error we specialize in; let the caller decide.
                throw exception;
        }
    }

    /// <summary>
    /// Attempts to report <paramref name="exception"/> as a known, user-facing
    /// error (a Fargo API error or a connectivity failure). Returns
    /// <c>true</c> and sets <paramref name="exitCode"/> when handled; returns
    /// <c>false</c> for exceptions that should propagate.
    /// </summary>
    public static bool TryReport(Exception exception, out int exitCode)
    {
        if (exception is not FargoApiException && !IsConnectivityError(exception))
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
