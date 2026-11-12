namespace Fargo.Core.Identity.Credentials;

public readonly struct PasswordHash : IEquatable<PasswordHash>
{
    public const int MaxLength = 512;

    /// <summary>
    /// For persistence and verification only; do not expose it to clients or write it to logs.
    /// </summary>
    public string Value { get; }

    public PasswordHash(string value)
    {
        Validate(value);
        Value = value;
    }

    public bool Equals(PasswordHash other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is PasswordHash other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(PasswordHash left, PasswordHash right)
        => left.Equals(right);

    public static bool operator !=(PasswordHash left, PasswordHash right)
        => !left.Equals(right);

    /// <summary>
    /// Always returns <c>[REDACTED]</c>.
    /// </summary>
    public override string ToString()
        => "[REDACTED]";

    public static explicit operator PasswordHash(string value)
        => new(value);

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Password hash cannot be null or empty.",
                nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Password hash length must not be greater than {MaxLength} characters.");
        }
    }
}
