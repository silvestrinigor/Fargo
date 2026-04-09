namespace Fargo.Sdk.Authentication;

public sealed class AuthSession : IAuthSession
{
    public string? Nameid { get; private set; } = default;

    public string? AccessToken { get; private set; } = default;

    public string? RefreshToken { get; private set; } = default;

    public DateTimeOffset? ExpiresAt { get; private set; } = default;

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;

    internal void SetTokens(string nameid, string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        Nameid = nameid;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    internal void Clear()
    {
        Nameid = default;
        AccessToken = default;
        RefreshToken = default;
        ExpiresAt = default;
    }
}
