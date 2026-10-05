namespace Fargo.Core.Identity;

public interface IAuthenticationAttemptRepository
{
    void Add(AuthenticationAttempt authenticationAttempt);

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
