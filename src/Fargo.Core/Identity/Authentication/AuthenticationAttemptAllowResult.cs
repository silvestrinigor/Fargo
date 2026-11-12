namespace Fargo.Core.Identity.Authentication;

/// <summary>
/// Outcome of the rate-limit check. <paramref name="RetryAfter"/> is <see cref="TimeSpan.Zero"/>
/// when <paramref name="Allow"/> is <see langword="true"/>.
/// </summary>
public sealed record AuthenticationAttemptAllowResults(
    bool Allow,
    TimeSpan RetryAfter
);
