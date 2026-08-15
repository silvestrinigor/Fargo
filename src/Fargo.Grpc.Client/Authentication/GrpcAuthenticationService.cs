using Fargo.Grpc.V1;

namespace Fargo.Grpc.Client.Authentication;

public sealed class GrpcAuthenticationService(
    IdentityService.IdentityServiceClient client,
    ITokenStore tokenStore)
    : IGrpcAuthenticationService
{
    public async Task LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await client.LoginAsync(
            new LoginRequest
            {
                Nameid = username,
                Password = password
            },
            cancellationToken: cancellationToken);

        tokenStore.SetTokens(
            response.AccessToken, response.RefreshToken, response.ExpiresAt.ToDateTimeOffset());
    }

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        var current = tokenStore.RefreshToken
            ?? throw new InvalidOperationException(
                "The client is not authenticated.");

        var response = await client.RefreshTokenAsync(
            new RefreshTokenRequest
            {
                RefreshToken = current
            },
            cancellationToken: cancellationToken);

        tokenStore.SetTokens(
            response.AccessToken, response.RefreshToken, response.ExpiresAt.ToDateTimeOffset());
    }

    public async Task LogoutAsync(
        CancellationToken cancellationToken = default)
    {
        var current = tokenStore.RefreshToken
            ?? throw new InvalidOperationException(
                "The client is not authenticated.");

        try
        {
            await client.LogoutAsync(
                new LogoutRequest
                {
                    RefreshToken = current
                },
                cancellationToken: cancellationToken);
        }
        finally
        {
            tokenStore.Clear();
        }
    }
}
