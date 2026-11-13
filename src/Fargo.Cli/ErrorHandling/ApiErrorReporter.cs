using Fargo.Client.ErrorHandling;
using System.Net.Sockets;

namespace Fargo.Cli.ErrorHandling;

/// <summary>
/// Renders Fargo API errors and connectivity failures as concise console messages and maps them to
/// exit codes, so every command reports failures the same way.
/// </summary>
public static class ApiErrorReporter
{
    /// <summary>
    /// Set by <c>--verbose</c>: error output also includes exception types, inner exceptions and the
    /// stack trace.
    /// </summary>
    public static bool Verbose { get; set; }

    /// <summary>
    /// Writes a message for the exception to standard error and returns the exit code. Throws the
    /// exception again when <see cref="CanReport"/> is <see langword="false"/>.
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
    /// Reports whether <see cref="Report"/> can render the exception. Has no side effects, so it is safe
    /// in an exception filter.
    /// </summary>
    public static bool CanReport(Exception exception) =>
        exception is FargoApiException || IsConnectivityError(exception);

    /// <summary>
    /// Calls <see cref="Report"/> when <see cref="CanReport"/> accepts the exception; otherwise returns
    /// <see langword="false"/> so the exception propagates.
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
                case IOException:
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
    /// an error a command has already reported in its own words.
    /// </summary>
    public static void WriteVerboseDiagnostics(Exception exception) => WriteVerboseDetails(exception);

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
