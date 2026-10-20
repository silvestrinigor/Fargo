namespace Fargo.Core.Identity;

/// <summary>
/// Defines the contract for tracking authentication attempts and enforcing
/// rate limiting.
/// </summary>
public interface IAuthenticationAttemptService
{
    /// <summary>
    /// Determines whether an authentication attempt is currently allowed for the
    /// specified identifier and IP address.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the actor attempting to authenticate.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address from which the attempt originates.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An <see cref="AuthenticationAttemptAllowResults"/> describing whether the
    /// attempt is allowed and, if not, how long the caller should wait before
    /// retrying.
    /// </returns>
    Task<AuthenticationAttemptAllowResults> IsAllowedAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Registers a failed authentication attempt.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the actor that attempted to authenticate.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address from which the attempt originated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    Task RegisterFailureAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Registers a successful authentication attempt.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the actor that authenticated.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address from which the attempt originated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    Task RegisterSuccessAsync(
        string identifier,
        string ipAddress,
        CancellationToken cancellationToken
    );
}
