namespace Fargo.Core.Identity;

/// <summary>
/// Defines the repository contract for managing <see cref="AuthenticationAttempt"/> entities.
/// </summary>
public interface IAuthenticationAttemptRepository
{
    /// <summary>
    /// Adds a new authentication attempt to the persistence context.
    /// </summary>
    /// <param name="authenticationAttempt">
    /// The authentication attempt to add.
    /// </param>
    void Add(AuthenticationAttempt authenticationAttempt);

    /// <summary>
    /// Gets the number of failed authentication attempts matching the specified
    /// IP address and identifier.
    /// </summary>
    /// <param name="ipAddress">
    /// The IP address from which the attempts originated.
    /// </param>
    /// <param name="identifier">
    /// The identifier of the actor that attempted to authenticate.
    /// </param>
    /// <param name="periodStart">
    /// The start of the period to consider. When <see langword="null"/>, all
    /// recorded attempts are considered.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="AuthenticationAttemptCountInformation"/> describing the
    /// number of failed attempts.
    /// </returns>
    Task<AuthenticationAttemptCountInformation> GetAuthenticationAttemptFailCountAsync(
        string ipAddress,
        string identifier,
        DateTimeOffset? periodStart = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the most recent successful authentication attempt matching the
    /// specified IP address and identifier.
    /// </summary>
    /// <param name="ipAddress">
    /// The IP address from which the attempt originated.
    /// </param>
    /// <param name="identifier">
    /// The identifier of the actor that attempted to authenticate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The most recent successful <see cref="AuthenticationAttempt"/> if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<AuthenticationAttempt?> GetLastSuccessAuthenticationAttemptAsync(
        string ipAddress,
        string identifier,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the most recent failed authentication attempt matching the
    /// specified IP address and identifier.
    /// </summary>
    /// <param name="ipAddress">
    /// The IP address from which the attempt originated.
    /// </param>
    /// <param name="identifier">
    /// The identifier of the actor that attempted to authenticate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The most recent failed <see cref="AuthenticationAttempt"/> if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<AuthenticationAttempt?> GetLastFailAuthenticationAttemptAsync(
        string ipAddress,
        string identifier,
        CancellationToken cancellationToken = default
    );
}
