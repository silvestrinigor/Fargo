namespace Fargo.Core.Identity;

/// <summary>
/// Represents the result of evaluating whether an authentication attempt is allowed.
/// </summary>
/// <param name="Allow">
/// A value indicating whether the authentication attempt is allowed.
/// </param>
/// <param name="RetryAfter">
/// The amount of time the caller should wait before retrying when
/// <paramref name="Allow"/> is <see langword="false"/>; otherwise,
/// <see cref="TimeSpan.Zero"/>.
/// </param>
public sealed record AuthenticationAttemptAllowResults(
    bool Allow,
    TimeSpan RetryAfter
);
