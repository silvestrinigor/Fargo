

namespace Fargo.Core.Tokens;

#region Values

/// <summary>
/// Represents a security token used by the system.
///
/// Tokens are typically generated for authentication or authorization
/// purposes (for example, access tokens or refresh tokens).
/// This value object ensures the token is not null, empty, or malformed.
/// </summary>
public readonly struct Token : IEquatable<Token>
{
    /// <summary>
    /// Minimum allowed length for the token.
    /// </summary>
    public const int MinLength = 50;

    /// <summary>
    /// Maximum allowed length for the token.
    /// </summary>
    public const int MaxLength = 4096;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> value object.
    /// </summary>
    /// <param name="value">The token string.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains invalid characters.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the token length is outside the allowed range.
    /// </exception>
    public Token(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying token value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct is not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Token value must be set.");

    /// <summary>
    /// Creates a <see cref="Token"/> from the specified string.
    /// </summary>
    public static Token FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current token is equal to another token.
    /// </summary>
    public bool Equals(Token other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current token is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Token other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current token.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Token"/> instances are equal.
    /// </summary>
    public static bool operator ==(Token left, Token right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Token"/> instances are different.
    /// </summary>
    public static bool operator !=(Token left, Token right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the token string.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Token"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(Token token)
        => token.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="Token"/>.
    /// </summary>
    public static explicit operator Token(string value)
        => new(value);

    /// <summary>
    /// Validates the token value.
    /// </summary>
    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Token cannot be null or empty.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Token length must be between {MinLength} and {MaxLength} characters.");
        }

        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                throw new ArgumentException(
                    "Token cannot contain whitespace.",
                    nameof(value));
            }
        }
    }
}
/// <summary>
/// Represents a hashed token stored by the system.
/// </summary>
/// <remarks>
/// This value object ensures that only a valid token hash is stored.
/// The original token must never be persisted — only its hash.
///
/// The hash value is normalized to uppercase when stored, ensuring a
/// canonical representation and enabling efficient ordinal comparisons.
///
/// Validation ignores case differences and rejects whitespace.
/// </remarks>
public readonly struct TokenHash : IEquatable<TokenHash>
{
    /// <summary>
    /// Minimum allowed length for the token hash.
    /// </summary>
    public const int MinLength = 50;

    /// <summary>
    /// Maximum allowed length for the token hash.
    /// </summary>
    public const int MaxLength = 512;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenHash"/> value object.
    /// </summary>
    /// <param name="value">The hashed token value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the hash length is outside the allowed range.
    /// </exception>
    public TokenHash(string value)
    {
        Validate(value);

        // Normalize to canonical uppercase representation
        this.value = value.ToUpperInvariant();
    }

    /// <summary>
    /// Gets the underlying hash value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Token hash value must be set.");

    /// <summary>
    /// Creates a <see cref="TokenHash"/> from the specified string.
    /// </summary>
    /// <param name="value">The hashed token value.</param>
    /// <returns>A new <see cref="TokenHash"/> instance.</returns>
    public static TokenHash FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current token hash is equal to another token hash.
    /// </summary>
    /// <param name="other">The other token hash to compare.</param>
    /// <returns><see langword="true"/> if both hashes are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(TokenHash other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current token hash is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is TokenHash other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current token hash.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="TokenHash"/> instances are equal.
    /// </summary>
    public static bool operator ==(TokenHash left, TokenHash right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="TokenHash"/> instances are different.
    /// </summary>
    public static bool operator !=(TokenHash left, TokenHash right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the token hash.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="TokenHash"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(TokenHash tokenHash)
        => tokenHash.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="TokenHash"/>.
    /// </summary>
    public static explicit operator TokenHash(string value)
        => new(value);

    /// <summary>
    /// Validates the token hash value.
    /// </summary>
    /// <param name="value">The hash value to validate.</param>
    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Token hash cannot be null or empty.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Token hash length must be between {MinLength} and {MaxLength} characters.");
        }

        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                throw new ArgumentException(
                    "Token hash cannot contain whitespace.",
                    nameof(value));
            }
        }
    }
}

#endregion Values

#region Entities

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
    public required Guid UserGuid { get; init; }

    /// <summary>
    /// Gets the hashed value of the refresh token.
    ///
    /// The raw token should never be stored in the database. Only the hash
    /// is persisted for security purposes.
    /// </summary>
    public required TokenHash TokenHash { get; init; }

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

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the hash of the token that replaced this token during rotation.
    ///
    /// When refresh token rotation is enabled, a used refresh token
    /// is replaced by a new one and this property stores the hash
    /// of the replacement token.
    /// </summary>
    public TokenHash? ReplacedByTokenHash { get; private set; } = null;

    /// <summary>
    /// Gets a value indicating whether the refresh token is expired.
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

    public bool IsUsable => !IsExpired && RevokedAt is null && ReplacedByTokenHash is null;

    public void ReplaceWith(TokenHash replacementTokenHash)
    {
        ReplacedByTokenHash = replacementTokenHash;
        RevokedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}

#endregion Entities

#region Repositories

/// <summary>
/// Defines the repository contract for managing <see cref="RefreshToken"/> entities.
///
/// This repository provides access to refresh token persistence operations
/// and queries used during authentication flows.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the refresh token.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="RefreshToken"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<RefreshToken?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a refresh token by its hashed token value.
    /// </summary>
    /// <param name="tokenHash">The hashed token used to identify the refresh token.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="RefreshToken"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<RefreshToken?> GetByTokenHash(
        TokenHash tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RefreshToken>> GetByUserGuid(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token to the persistence context.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    void Add(RefreshToken refreshToken);

    /// <summary>
    /// Removes a refresh token from the persistence context.
    /// </summary>
    /// <param name="refreshToken">The refresh token to remove.</param>
    void Remove(RefreshToken refreshToken);
}

#endregion Repositories

#region Services

/// <summary>
/// Defines the contract for generating refresh tokens used
/// in the authentication system.
///
/// Implementations are responsible for producing secure,
/// cryptographically random tokens that can later be hashed
/// and stored by the system.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>A newly generated <see cref="Token"/>.</returns>
    Token Generate();
}
/// <summary>
/// Defines the contract for hashing security tokens.
///
/// Implementations are responsible for producing a deterministic
/// hash of a token so that the system can safely store the hash
/// instead of the plaintext token.
/// </summary>
public interface ITokenHasher
{
    /// <summary>
    /// Generates a hash for the specified token.
    /// </summary>
    /// <param name="token">The plaintext token.</param>
    /// <returns>A <see cref="TokenHash"/> representing the hashed token.</returns>
    TokenHash Hash(Token token);
}

#endregion Services
