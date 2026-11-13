using Microsoft.AspNetCore.SignalR.Client;

namespace Fargo.Client.Realtime;

/// <summary>
/// Retries without limit: immediately first, then after 1, 2, 4… seconds up to
/// a maximum.
/// </summary>
/// <remarks>
/// The SignalR default gives up after four attempts (0, 2, 10 and 30 seconds),
/// which leaves a long-running client disconnected for good after a short
/// outage.
/// </remarks>
internal sealed class ReconnectPolicy(TimeSpan maxDelay) : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext) => GetDelay(retryContext.PreviousRetryCount);

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
