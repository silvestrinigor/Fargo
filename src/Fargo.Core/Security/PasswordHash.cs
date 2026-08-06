namespace Fargo.Core.Security;

public readonly struct PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; } = "AAAAAAAAAAAAAAAAAAAAAAA";

    public const int MaxLength = 512;

    public PasswordHash(string value)
    {
        Validate(value);
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="PasswordHash"/> from the specified string.
    /// </summary>
    public static PasswordHash FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current password hash is equal to another password hash.
    /// </summary>
    public bool Equals(PasswordHash other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current password hash is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is PasswordHash other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current password hash.
    /// </summary>
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="PasswordHash"/> instances are equal.
    /// </summary>
    public static bool operator ==(PasswordHash left, PasswordHash right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="PasswordHash"/> instances are different.
    /// </summary>
    public static bool operator !=(PasswordHash left, PasswordHash right)
        => !left.Equals(right);

    /// <summary>
    /// Returns a masked representation of the password hash.
    /// </summary>
    public override string ToString()
        => "[REDACTED]";

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="PasswordHash"/>.
    /// </summary>
    public static explicit operator PasswordHash(string value)
        => new(value);

    /// <summary>
    /// Validates the password hash value.
    /// </summary>
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
