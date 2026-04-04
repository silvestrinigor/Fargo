namespace Fargo.Sdk.Security;

public sealed class FargoAuthSession
{
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public void SetTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    public void Clear()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        ExpiresAt = default;
    }
}
