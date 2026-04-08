namespace Fargo.Sdk.Authentication;

public sealed class AuthSession : IAuthSession
{
    public string? AccessToken { get; private set; } = default;

    public string? RefreshToken { get; private set; } = default;

    public DateTimeOffset? ExpiresAt { get; private set; } = default;

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    internal void SetTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    internal void Clear()
    {
        AccessToken = default;
        RefreshToken = default;
        ExpiresAt = default;
    }
}
