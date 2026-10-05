namespace Fargo.Http.Client.Authentication;

public interface ITokenStore
{
    Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default);

    Task SetAsync(
        AuthTokens tokens,
        CancellationToken cancellationToken = default
    );

    Task ClearAsync(CancellationToken cancellationToken = default);
}
