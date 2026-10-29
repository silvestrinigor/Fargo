using Fargo.ClientHttp.ErrorHandling;

namespace Fargo.WorkflowTest;

/// <summary>
/// Lightweight assertion helper for the workflow driver. Instead of throwing on
/// the first failure, it records pass/fail counts and logs each outcome so a run
/// reports every problem it found. <see cref="Failures"/> drives the process
/// exit code.
/// </summary>
internal sealed class Check
{
    public int Passed { get; private set; }

    public int Failures { get; private set; }

    /// <summary>Asserts a boolean condition.</summary>
    public void That(bool condition, string description)
    {
        if (condition)
        {
            Pass(description);
        }
        else
        {
            Fail(description);
        }
    }

    /// <summary>Asserts two values are equal.</summary>
    public void Equal<T>(T expected, T actual, string description)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            Pass($"{description} (= {expected})");
        }
        else
        {
            Fail($"{description} (expected {expected}, got {actual})");
        }
    }

    /// <summary>
    /// Asserts that the operation completes without throwing a Fargo API error.
    /// Use for "this actor SHOULD be allowed" cases.
    /// </summary>
    public async Task AllowedAsync(string description, Func<Task> operation)
    {
        try
        {
            await operation();
            Pass($"allowed: {description}");
        }
        catch (FargoApiException ex)
        {
            Fail($"allowed: {description} — but was rejected ({ex.StatusCode}: {ex.Message})");
        }
    }

    /// <summary>
    /// Asserts that the operation is rejected by the server with one of the
    /// expected HTTP status codes (e.g. 403 Forbidden for a missing permission,
    /// 404 Not Found for a partition-scoped read the actor cannot see). Use for
    /// "this actor SHOULD be denied" cases.
    /// </summary>
    public async Task DeniedAsync(string description, Func<Task> operation, params int[] expectedStatuses)
    {
        var expected = expectedStatuses.Length > 0 ? expectedStatuses : [401, 403];

        try
        {
            await operation();
            Fail($"denied: {description} — but the operation succeeded");
        }
        catch (FargoApiException ex) when (expected.Contains(ex.StatusCode))
        {
            Pass($"denied: {description} ({ex.StatusCode} as expected)");
        }
        catch (FargoApiException ex)
        {
            Fail($"denied: {description} — rejected with unexpected status {ex.StatusCode} " +
                 $"(expected {string.Join("/", expected)}): {ex.Message}");
        }
    }

    private void Pass(string description)
    {
        Passed++;
        Log.Success(description);
    }

    private void Fail(string description)
    {
        Failures++;
        Log.Error(description);
    }
}
