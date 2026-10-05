namespace Fargo.Core.Identity;

public sealed class AuthenticationAttemptService(IAuthenticationAttemptRepository attemptRepository) : IAuthenticationAttemptService
{
    public async Task<AuthenticationAttemptAllowResults> IsAllowedAsync(string identifier, string ipAddress, CancellationToken cancellationToken)
    {
        var lastSuccessAuth = await attemptRepository.GetLastSuccessAuthenticationAttemptAsync(ipAddress, identifier, cancellationToken);

        var attemptCount = await attemptRepository.GetAuthenticationAttemptFailCountAsync(ipAddress, identifier, lastSuccessAuth?.OccuredAt ?? null, cancellationToken);

        var lastFailAuth = await attemptRepository.GetLastFailAuthenticationAttemptAsync(ipAddress, identifier, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var retryAfter = TimeSpan.Zero;

        if (attemptCount.IpAddressAndUserIdentifierAttemptCount >= 5 &&
            lastFailAuth?.OccuredAt >= now - TimeSpan.FromMinutes(5))
        {
            var remaining = TimeSpan.FromMinutes(5) - (now - lastFailAuth.OccuredAt);
            retryAfter = Max(retryAfter, remaining);
        }

        if (attemptCount.IpAddressAttemptCount >= 20 &&
            lastFailAuth?.OccuredAt >= now - TimeSpan.FromMinutes(1))
        {
            var remaining = TimeSpan.FromMinutes(1) - (now - lastFailAuth.OccuredAt);
            retryAfter = Max(retryAfter, remaining);
        }

        if (attemptCount.ActorIdentifierAttemptCount >= 5 &&
            lastFailAuth?.OccuredAt >= now - TimeSpan.FromMinutes(15))
        {
            var remaining = TimeSpan.FromMinutes(15) - (now - lastFailAuth.OccuredAt);
            retryAfter = Max(retryAfter, remaining);
        }

        if (retryAfter > TimeSpan.Zero)
        {
            return new AuthenticationAttemptAllowResults(false, retryAfter);
        }

        return new AuthenticationAttemptAllowResults(true, TimeSpan.Zero);

        static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;
    }

    public Task RegisterFailureAsync(string identifier, string ipAddress, CancellationToken cancellationToken)
    {
        var attempt = new AuthenticationAttempt(identifier, ipAddress, successful: false);

        attemptRepository.Add(attempt);

        return Task.CompletedTask;
    }

    public Task RegisterSuccessAsync(string identifier, string ipAddress, CancellationToken cancellationToken)
    {
        var attempt = new AuthenticationAttempt(identifier, ipAddress, successful: true);

        attemptRepository.Add(attempt);

        return Task.CompletedTask;
    }
}
