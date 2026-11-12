namespace Fargo.Core.Identity.Tokens;

/// <summary>
/// The hash of a token: at most 512 characters, no whitespace. The original token must never be
/// persisted, only its hash.
/// </summary>
public readonly struct TokenHash : IEquatable<TokenHash>
{
    public const int MaxLength = 512;

    public string Value { get; }

    public TokenHash(string value)
    {
        Validate(value);

        Value = value;
    }

    public static TokenHash FromString(string value)
        => new(value);

    public bool Equals(TokenHash other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is TokenHash other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(TokenHash left, TokenHash right)
        => left.Equals(right);

    public static bool operator !=(TokenHash left, TokenHash right)
        => !left.Equals(right);

    /// <summary>
    /// Always returns <c>[REDACTED]</c>.
    /// </summary>
    public override string ToString() => "[REDACTED]";

    public static explicit operator TokenHash(string value)
        => new(value);

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Token hash cannot be null or empty.",
                nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Token hash length must be lower or equal {MaxLength} characters.");
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
