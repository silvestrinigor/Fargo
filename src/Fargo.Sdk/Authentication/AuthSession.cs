namespace Fargo.Sdk.Authentication;

public sealed class AuthSession : IAuthSession
{
    private sealed record Snapshot(string Nameid, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

    private volatile Snapshot? snapshot;

    public string? Nameid => snapshot?.Nameid;

    public string? AccessToken => snapshot?.AccessToken;

    public string? RefreshToken => snapshot?.RefreshToken;

    public DateTimeOffset? ExpiresAt => snapshot?.ExpiresAt;

    public bool IsAuthenticated => snapshot is not null;

    public bool IsExpired => snapshot?.ExpiresAt < DateTimeOffset.UtcNow;

    internal void SetTokens(string nameid, string accessToken, string refreshToken, DateTimeOffset expiresAt)
        => snapshot = new Snapshot(nameid, accessToken, refreshToken, expiresAt);

    internal void Clear() => snapshot = null;
}
