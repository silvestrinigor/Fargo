using Fargo.ClientHttp.ErrorHandling;

namespace Fargo.SimulationWorker;

/// <summary>
/// Lightweight, thread-safe self-consistency recorder. Unlike a test assertion
/// library it never throws: it records pass/fail counts and logs each failure so
/// the simulation keeps running while still surfacing problems. The totals feed
/// the periodic and final metrics summaries.
/// </summary>
internal sealed class Check
{
    private long _passed;
    private long _failed;

    public long Passed => Interlocked.Read(ref _passed);

    public long Failed => Interlocked.Read(ref _failed);

    /// <summary>Records a boolean consistency expectation.</summary>
    public void That(string who, bool condition, string description)
    {
        if (condition)
        {
            Interlocked.Increment(ref _passed);
        }
        else
        {
            Interlocked.Increment(ref _failed);
            Log.Error(who, $"consistency: {description}");
        }
    }

    /// <summary>Records an equality consistency expectation.</summary>
    public void Equal<T>(string who, T expected, T actual, string description)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            Interlocked.Increment(ref _passed);
        }
        else
        {
            Interlocked.Increment(ref _failed);
            Log.Error(who, $"consistency: {description} (expected {expected}, got {actual})");
        }
    }

    /// <summary>
    /// Asserts an operation the actor should be allowed to perform completes
    /// without a Fargo API error. A rejection is recorded as a failure.
    /// </summary>
    public async Task AllowedAsync(string who, string description, Func<Task> operation)
    {
        try
        {
            await operation();
            Interlocked.Increment(ref _passed);
        }
        catch (FargoApiException ex)
        {
            Interlocked.Increment(ref _failed);
            Log.Error(who, $"expected-allowed: {description} — rejected ({ex.StatusCode}: {ex.Message})");
        }
    }
}
