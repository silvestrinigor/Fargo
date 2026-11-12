namespace Fargo.Core.Identity.Authentication;

public interface IAuthenticationAttemptService
{
    /// <summary>
    /// Checks whether an attempt is currently allowed; if not, the result says how long to wait.
    /// </summary>
    Task<AuthenticationAttemptAllowResults> IsAllowedAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Records a failed attempt. The attempt is only added to the repository, not saved.
    /// </summary>
    Task RegisterFailureAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Records a successful attempt, which resets the failure count. The attempt is only added to the
    /// repository, not saved.
    /// </summary>
    Task RegisterSuccessAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );
}
