namespace Fargo.Grpc.Client.Authentication;

public class TokenStore : ITokenStore
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public void Clear()
    {
        AccessToken = null;

        RefreshToken = null;

        ExpiresAt = null;
    }

    public void SetTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;

        RefreshToken = refreshToken;

        ExpiresAt = expiresAt;
    }
}
