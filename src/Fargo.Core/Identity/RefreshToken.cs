using Fargo.Core.Entities;

namespace Fargo.Core.Identity;

/// <summary>
/// Represents a refresh token used to obtain a new access token
/// without requiring the user to authenticate again.
/// </summary>
/// <remarks>
/// Refresh tokens are long-lived credentials associated with a user.
/// Only the hashed token value is persisted; the original token value
/// must not be stored.
///
/// A refresh token can become unusable when it expires or is revoked.
/// </remarks>
public sealed class RefreshToken : IEntity
{
    /// <summary>
    /// Default number of days before a refresh token expires.
    /// </summary>
    private const short defaultExpirationDays = 10;

    /// <summary>
    /// Gets the unique identifier of the refresh token.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the default expiration duration applied to newly created refresh tokens.
    /// </summary>
    public static TimeSpan DefaultExpirationTimeSpan { get; } = TimeSpan.FromDays(defaultExpirationDays);

    /// <summary>
    /// Gets the unique identifier of the user associated with this refresh token.
    /// </summary>
    public Guid UserGuid { get; private init; }

    /// <summary>
    /// Gets the hashed value of the refresh token.
    /// </summary>
    /// <remarks>
    /// The original token value must never be persisted. The stored hash is used
    /// to validate refresh token requests without exposing the token itself.
    /// </remarks>
    public required TokenHash TokenHash { get; init; }

    /// <summary>
    /// Gets the date and time when the refresh token expires.
    /// </summary>
    /// <remarks>
    /// Newly created refresh tokens expire after
    /// <see cref="DefaultExpirationTimeSpan"/> from their creation time.
    /// </remarks>
    public DateTimeOffset ExpiresAt { get; init; } = DateTimeOffset.UtcNow + DefaultExpirationTimeSpan;

    /// <summary>
    /// Gets the date and time when the refresh token was revoked.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the refresh token has not
    /// been revoked.
    /// </remarks>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the refresh token has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the refresh token can currently be used
    /// to obtain a new access token.
    /// </summary>
    /// <remarks>
    /// A refresh token is usable only while it has not expired and has not been
    /// revoked.
    /// </remarks>
    public bool IsUsable => !IsExpired && RevokedAt is null;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// </summary>
    /// <remarks>
    /// Intended only for Entity Framework Core materialization.
    /// </remarks>
    private RefreshToken()
    {
    }

    /// <summary>
    /// Creates a new refresh token for the specified user.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user that owns the refresh token.
    /// </param>
    /// <param name="tokenHash">
    /// The hashed value of the refresh token.
    /// </param>
    /// <returns>
    /// A new <see cref="RefreshToken"/> instance.
    /// </returns>
    public static RefreshToken Create(Guid userGuid, TokenHash tokenHash)
        => new()
        {
            UserGuid = userGuid,
            TokenHash = tokenHash
        };

    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    /// <remarks>
    /// If the refresh token has already been revoked, this method has no effect.
    /// </remarks>
    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}
