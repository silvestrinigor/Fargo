using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities;

// TODO: validate documentation
/// <summary>
/// Represents a refresh token used to obtain a new access token
/// without requiring the user to authenticate again.
///
/// Refresh tokens are long-lived credentials associated with a user
/// and can be rotated or invalidated when replaced.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>
    /// Default number of days before a refresh token expires.
    /// </summary>
    private const short defaultExpirationDays = 10;

    /// <summary>
    /// Gets the default expiration duration for refresh tokens.
    /// </summary>
    public static TimeSpan DefaultExpirationTimeSpan
    {
        get;
    } = TimeSpan.FromDays(defaultExpirationDays);

    /// <summary>
    /// Gets the unique identifier of the user associated with this refresh token.
    /// </summary>
    public required Guid UserGuid
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the hashed value of the refresh token.
    ///
    /// The raw token should never be stored in the database. Only the hash
    /// is persisted for security purposes.
    /// </summary>
    public required TokenHash TokenHash
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the date and time when the refresh token expires.
    ///
    /// By default, this is set to the current UTC time plus
    /// <see cref="DefaultExpirationTimeSpan"/>.
    /// </summary>
    public DateTimeOffset ExpiresAt
    {
        get;
        init;
    } = DateTimeOffset.UtcNow + DefaultExpirationTimeSpan;

    /// <summary>
    /// Gets the hash of the token that replaced this token during rotation.
    ///
    /// When refresh token rotation is enabled, a used refresh token
    /// is replaced by a new one and this property stores the hash
    /// of the replacement token.
    /// </summary>
    public TokenHash? ReplacedByTokenHash
    {
        get;
        init;
    } = null;

    /// <summary>
    /// Gets a value indicating whether the refresh token is expired.
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
}
