using Fargo.ClientHttp.ErrorHandling;

namespace Fargo.Cli.ErrorHandling;

/// <summary>
/// Renders Fargo API errors as concise, user-friendly console messages and maps
/// them to process exit codes. Centralizes the handling of 401/403 responses so
/// every command surfaces the same clear output instead of raw stack traces.
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
                // Not an API error we specialize in; let the caller decide.
                throw exception;
        }
    }

    /// <summary>
    /// Attempts to report <paramref name="exception"/> as a known API error.
    /// Returns <c>true</c> and sets <paramref name="exitCode"/> when handled;
    /// returns <c>false</c> for exceptions that are not Fargo API errors.
    /// </summary>
    public static bool TryReport(Exception exception, out int exitCode)
    {
        if (exception is not FargoApiException)
        {
            exitCode = 0;
            return false;
        }

        exitCode = Report(exception);
        return true;
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
