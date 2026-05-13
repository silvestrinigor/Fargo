namespace Fargo.Core.Identity;

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
