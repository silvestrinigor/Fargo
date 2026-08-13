namespace Fargo.Grpc.Client.Services;

public class TokenStore : ITokenStore
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public void Clear()
    {
        AccessToken = null;

        RefreshToken = null;
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;

        RefreshToken = refreshToken;
    }
}
