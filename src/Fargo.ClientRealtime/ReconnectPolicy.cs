using Microsoft.AspNetCore.SignalR.Client;

namespace Fargo.ClientRealtime;

/// <summary>
/// Retries without limit: immediately first, then after 1, 2, 4… seconds up to
/// a maximum.
/// </summary>
/// <remarks>
/// The SignalR default gives up after four attempts (0, 2, 10 and 30 seconds),
/// which leaves a long-running client disconnected for good after a short
/// outage.
/// </remarks>
/// <param name="maxDelay">The longest wait between attempts.</param>
internal sealed class ReconnectPolicy(TimeSpan maxDelay) : IRetryPolicy
{
    /// <inheritdoc />
    public TimeSpan? NextRetryDelay(RetryContext retryContext) => GetDelay(retryContext.PreviousRetryCount);

    /// <summary>
    /// Gets the wait before the attempt that follows <paramref name="previousAttempts"/> failed ones.
    /// </summary>
    internal TimeSpan GetDelay(long previousAttempts)
    {
        if (previousAttempts <= 0)
        {
            return TimeSpan.Zero;
        }

        var exponent = Math.Min(previousAttempts - 1, 30);
        var ticks = TimeSpan.TicksPerSecond * Math.Pow(2, exponent);

        return ticks >= maxDelay.Ticks ? maxDelay : TimeSpan.FromTicks((long)ticks);
    }
}
