namespace Fargo.Core.Identity.Tokens;

/// <summary>
/// An opaque security token (such as an access or refresh token): 50 to 4096 characters with no
/// whitespace. The domain does not interpret its value.
/// </summary>
public readonly struct Token :
    IEquatable<Token>,
    IParsable<Token>
{
    public const int MinLength = 50;

    public const int MaxLength = 4096;

    public string Value { get; } = "00000000000000000000000000000000000000000000000000";

    public Token(string value)
    {
        Validate(value);
        Value = value;
    }

    /// <summary>
    /// Always returns <c>[REDACTED]</c>, so the token does not leak into logs.
    /// </summary>
    public override string ToString()
        => "[REDACTED]";

    #region Equality

    public bool Equals(Token other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Token other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(Token left, Token right)
        => left.Equals(right);

    public static bool operator !=(Token left, Token right)
        => !left.Equals(right);

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

    #endregion

    public static explicit operator Token(string value)
        => new(value);

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
