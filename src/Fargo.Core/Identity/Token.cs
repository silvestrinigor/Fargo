using System.Text;

namespace Fargo.Core.Shared.Identity;

/// <summary>
/// Represents a security token used by the system.
/// </summary>
/// <remarks>
/// Tokens are opaque security credentials typically used for authentication
/// or authorization purposes, such as access tokens and refresh tokens.
///
/// This value object validates the token's length and ensures that it does
/// not contain whitespace. The token value itself is treated as opaque and
/// is not interpreted by the domain.
/// </remarks>
public readonly struct Token :
    IEquatable<Token>,
    IParsable<Token>,
    ISpanParsable<Token>,
    IUtf8SpanParsable<Token>
{
    /// <summary>
    /// Minimum allowed length for a token.
    /// </summary>
    public const int MinLength = 50;

    /// <summary>
    /// Maximum allowed length for a token.
    /// </summary>
    public const int MaxLength = 4096;

    /// <summary>
    /// Gets the underlying token value.
    /// </summary>
    public string Value { get; } = "00000000000000000000000000000000000000000000000000";

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> value object.
    /// </summary>
    /// <param name="value">
    /// The token value.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the token length is outside the allowed range.
    /// </exception>
    public Token(string value)
    {
        Validate(value);
        Value = value;
    }

    /// <summary>
    /// Returns a redacted representation of the token.
    /// </summary>
    /// <returns>
    /// A redacted string that does not expose the underlying token value.
    /// </returns>
    public override string ToString()
        => "[REDACTED]";

    #region Equality

    /// <summary>
    /// Determines whether the current token is equal to another token.
    /// </summary>
    /// <param name="other">
    /// The token to compare with the current token.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both tokens contain the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Token other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current token is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current token.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a
    /// <see cref="Token"/> with the same value; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Token other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current token.
    /// </summary>
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

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

    #endregion

    #region Parsing

    /// <summary>
    /// Parses a string into a <see cref="Token"/>.
    /// </summary>
    /// <param name="s">The token string to parse.</param>
    /// <param name="provider">The format provider.</param>
    /// <returns>A validated <see cref="Token"/>.</returns>
    public static Token Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <summary>
    /// Attempts to parse a string into a <see cref="Token"/>.
    /// </summary>
    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        out Token result)
    {
        try
        {
            result = new Token(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Parses a character span into a <see cref="Token"/>.
    /// </summary>
    public static Token Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <summary>
    /// Attempts to parse a character span into a <see cref="Token"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out Token result)
    {
        try
        {
            result = new Token(s.ToString());
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Parses a UTF-8 byte span into a <see cref="Token"/>.
    /// </summary>
    public static Token Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

    /// <summary>
    /// Attempts to parse a UTF-8 byte span into a <see cref="Token"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out Token result)
    {
        try
        {
            result = new Token(Encoding.UTF8.GetString(utf8Text));
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    #endregion

    /// <summary>
    /// Explicitly converts a string to a <see cref="Token"/>.
    /// </summary>
    public static explicit operator Token(string value)
        => new(value);

    /// <summary>
    /// Validates the specified token value.
    /// </summary>
    /// <param name="value">
    /// The value to validate.
    /// </param>
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
