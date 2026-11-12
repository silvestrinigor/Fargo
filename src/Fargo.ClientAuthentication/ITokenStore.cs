namespace Fargo.ClientAuthentication;

/// <summary>
/// Holds the current <see cref="AuthTokens"/> of a client session.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Gets the stored tokens, or <see langword="null"/> when there are none.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the stored tokens.
    /// </summary>
    /// <param name="tokens">The new tokens.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task SetAsync(
        AuthTokens tokens,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes the stored tokens.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
