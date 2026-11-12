namespace Fargo.ClientAuthentication;

/// <summary>
/// An in-memory <see cref="ITokenStore"/>; the tokens live as long as the
/// instance.
/// </summary>
public sealed class TokenStore : ITokenStore
{
    private AuthTokens? authTokens = null;

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        authTokens = null;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(authTokens);
    }

    /// <inheritdoc />
    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        authTokens = tokens;

        return Task.CompletedTask;
    }
}
