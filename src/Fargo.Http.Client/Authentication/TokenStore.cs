namespace Fargo.Http.Client.Authentication;

public sealed class TokenStore : ITokenStore
{
    private AuthTokens? authTokens = null;

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        authTokens = null;

        return Task.CompletedTask;
    }

    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(authTokens);
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        authTokens = tokens;

        return Task.CompletedTask;
    }
}
