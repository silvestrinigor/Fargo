namespace Fargo.Http.Client.Authentication;

public sealed class TokenStore : ITokenStore
{
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
