namespace Fargo.Client.Authentication;

/// <summary>
/// Holds the current <see cref="AuthTokens"/> of a client session.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Gets the stored tokens, or <see langword="null"/> when there are none.
    /// </summary>
    Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default);

    Task SetAsync(
        AuthTokens tokens,
        CancellationToken cancellationToken = default
    );

    Task ClearAsync(CancellationToken cancellationToken = default);
}
