using Fargo.ClientHttp.ErrorHandling;

namespace Fargo.SimulationWorker;

/// <summary>
/// Wraps operator API calls so the simulation survives the server's per-user
/// rate limit (the <c>general-authenticated</c> policy, ~100 requests/minute).
///
/// On an HTTP 429 it retries the same call after an exponential backoff with
/// jitter, up to a capped maximum delay, recording each throttling event in the
/// shared metrics. Other Fargo API errors (4xx/5xx) are recorded as errors and
/// rethrown so the caller can decide how to proceed; unexpected exceptions
/// propagate unchanged.
/// </summary>
internal sealed class RateLimitPolicy(SimulationOptions options, Metrics metrics, Random random)
{
    private const int RateLimitedStatus = 429;

    private readonly SimulationOptions _options = options;
    private readonly Metrics _metrics = metrics;
    private readonly Random _random = random;

    /// <summary>Executes an operation that returns a value, with 429 backoff.</summary>
    public async Task<T?> ExecuteAsync<T>(Func<Task<T?>> operation, CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await operation();
            }
            catch (FargoApiException ex) when (ex.StatusCode == RateLimitedStatus)
            {
                _metrics.RecordRateLimited();
                await BackoffAsync(++attempt, cancellationToken);
            }
        }
    }

    /// <summary>Executes an operation with no return value, with 429 backoff.</summary>
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        await ExecuteAsync<object?>(async () =>
        {
            await operation();
            return null;
        }, cancellationToken);
    }

    private async Task BackoffAsync(int attempt, CancellationToken cancellationToken)
    {
        // Exponential growth capped at the configured maximum, plus 0–250 ms
        // jitter so many operators do not retry in lockstep.
        var exponential = _options.RateLimitBaseBackoffMs * Math.Pow(2, Math.Min(attempt - 1, 8));
        var capped = Math.Min(exponential, _options.RateLimitMaxBackoffMs);
        var delay = (int)capped + _random.Next(0, 250);

        await Task.Delay(delay, cancellationToken);
    }
}
