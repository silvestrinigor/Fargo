namespace Fargo.Core.Identity.Tokens;

/// <summary>
/// A long-lived credential used to obtain a new access token without authenticating again. Only
/// the hash of the token is persisted. It is unusable once expired or revoked.
/// </summary>
public sealed class RefreshToken
{
    private const short defaultExpirationDays = 10;

    public Guid Guid { get; private init; } = Guid.NewGuid();

    public static TimeSpan DefaultExpirationTimeSpan { get; } = TimeSpan.FromDays(defaultExpirationDays);

    public Guid UserGuid { get; private init; }

    public required TokenHash TokenHash { get; init; }

    public DateTimeOffset ExpiresAt { get; private init; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// A token is already expired at exactly <see cref="ExpiresAt"/>.
    /// </summary>
    public bool IsExpiredAt(DateTimeOffset now) => ExpiresAt <= now;

    public bool IsUsableAt(DateTimeOffset now) => !IsExpiredAt(now) && RevokedAt is null;

    // Required by EF Core.
    private RefreshToken()
    {
    }

    /// <summary>
    /// The token expires <see cref="DefaultExpirationTimeSpan"/> after <paramref name="createdAt"/>.
    /// </summary>
    public static RefreshToken Create(Guid userGuid, TokenHash tokenHash, DateTimeOffset createdAt)
        => new()
        {
            UserGuid = userGuid,
            TokenHash = tokenHash,
            ExpiresAt = createdAt + DefaultExpirationTimeSpan,
        };

    /// <summary>
    /// Idempotent: revoking an already revoked token keeps the original revocation time.
    /// </summary>
    public void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt ??= revokedAt;
    }
}
