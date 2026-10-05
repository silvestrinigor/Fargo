namespace Fargo.Core.Identity;

public sealed record AuthenticationAttemptAllowResults(
    bool Allow,
    TimeSpan RetryAfter
);
