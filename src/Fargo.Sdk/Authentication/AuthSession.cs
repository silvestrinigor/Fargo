namespace Fargo.Sdk.Authentication;

public sealed class AuthSession
{
    public string AccessToken { get; private set; } = string.Empty;

    public string RefreshToken { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    internal void SetTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    internal void Clear()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        ExpiresAt = default;
    }
}
