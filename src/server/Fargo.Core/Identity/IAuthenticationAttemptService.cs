namespace Fargo.Core.Identity;

public interface IAuthenticationAttemptService
{
    Task<AuthenticationAttemptAllowResults> IsAllowedAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    Task RegisterFailureAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    Task RegisterSuccessAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );
}
