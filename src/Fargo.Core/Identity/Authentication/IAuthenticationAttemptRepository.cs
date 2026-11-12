namespace Fargo.Core.Identity.Authentication;

public interface IAuthenticationAttemptRepository
{
    void Add(AuthenticationAttempt authenticationAttempt);

    /// <summary>
    /// Counts failed attempts. When <paramref name="periodStart"/> is <see langword="null"/>, all
    /// recorded attempts are considered.
    /// </summary>
    Task<AuthenticationAttemptCountInformation> GetAuthenticationAttemptFailCountAsync(
        string ipAddress,
        string identifier,
        DateTimeOffset? periodStart = null,
        CancellationToken cancellationToken = default
    );

    Task<AuthenticationAttempt?> GetLastSuccessAuthenticationAttemptAsync(
        string ipAddress,
        string identifier,
        CancellationToken cancellationToken = default
    );

    Task<AuthenticationAttempt?> GetLastFailAuthenticationAttemptAsync(
        string ipAddress,
        string identifier,
        CancellationToken cancellationToken = default
    );
}
