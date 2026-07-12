using System.Text;

namespace Fargo.Core.Shared.Identity;

/// <summary>
/// Represents a security token used by the system.
///
/// Tokens are typically generated for authentication or authorization
/// purposes (for example, access tokens or refresh tokens).
/// This value object ensures the token is not null, empty, or malformed.
/// </summary>
public readonly struct Token :
    IEquatable<Token>,
    IParsable<Token>,
    ISpanParsable<Token>,
    IUtf8SpanParsable<Token>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanFormattable
{
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

    #region Equality

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

    #endregion

    #region Formatting

    /// <summary>
    /// Returns the token string.
    /// </summary>
    public override string ToString()
        => Value;

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value;

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (Value.AsSpan().TryCopyTo(destination))
        {
            charsWritten = Value.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        return Encoding.UTF8.TryGetBytes(Value.AsSpan(), utf8Destination, out bytesWritten);
    }

    #endregion

    #region Parsing

    public static Token Parse(string s, IFormatProvider? provider)
        => new(s);

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

    public static Token Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

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

    public static Token Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

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
