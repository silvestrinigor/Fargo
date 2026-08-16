using Fargo.Grpc.Client.Authentication;

public sealed class AccessTokenProvider(
    ITokenStore tokenStore,
    IGrpcAuthenticationService authenticationService)
    : IAccessTokenProvider
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<string?> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (tokenStore.AccessToken is null)
        {
            return null;
        }

        if (tokenStore.ExpiresAt >
            DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return tokenStore.AccessToken;
        }

        await refreshLock.WaitAsync(cancellationToken);

        try
        {
            if (tokenStore.AccessToken is null)
            {
                return null;
            }

            if (tokenStore.ExpiresAt >
                DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return tokenStore.AccessToken;
            }

            await authenticationService.RefreshAsync(
                cancellationToken);

            return tokenStore.AccessToken;
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
