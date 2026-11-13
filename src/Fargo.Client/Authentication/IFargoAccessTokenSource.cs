namespace Fargo.Client.Authentication;

/// <summary>
/// Provides the access token the Fargo HTTP client sends with each request.
/// </summary>
/// <remarks>
/// The default, <see cref="StoredAccessTokenSource"/>, returns the stored token
/// as is. Register <see cref="RefreshingAccessTokenSource"/> to refresh before
/// expiry, or a custom source, for example for a service account that obtains
/// its tokens with client credentials, or a fixed token.
/// </remarks>
public interface IFargoAccessTokenSource
{
    /// <summary>
    /// Gets the access token to send, or <see langword="null"/> when there is none.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Returns the access token held by the <see cref="ITokenStore"/>, without
/// refreshing it.
/// </summary>
public sealed class StoredAccessTokenSource(ITokenStore tokenStore) : IFargoAccessTokenSource
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => (await tokenStore.GetAsync(cancellationToken))?.AccessToken;
}

/// <summary>
/// Refreshes the stored tokens when they are about to expire
/// (<see cref="FargoTokenRefresher.EnsureValidAsync"/>), then returns the
/// access token. Opt in with
/// <c>UseFargoRefreshingAccessTokens()</c>.
/// </summary>
public sealed class RefreshingAccessTokenSource(FargoTokenRefresher refresher, ITokenStore tokenStore) : IFargoAccessTokenSource
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await refresher.EnsureValidAsync(cancellationToken);

        return (await tokenStore.GetAsync(cancellationToken))?.AccessToken;
    }
}
