namespace Fargo.Domain.Users;

/// <summary>
/// Represents a hashed password stored in the system.
///
/// This value object guarantees that the stored value is a valid
/// password hash produced by the hashing infrastructure.
/// The plaintext password should never be persisted.
/// </summary>
public readonly struct PasswordHash : IEquatable<PasswordHash>
{
    /// <summary>
    /// Minimum allowed length for the password hash.
    /// </summary>
    public const int MinLength = 50;

    /// <summary>
    /// Maximum allowed length for the password hash.
    /// </summary>
    public const int MaxLength = 512;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHash"/> value object.
    /// </summary>
    /// <param name="value">The hashed password value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains invalid characters.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public PasswordHash(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying hash string.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct is not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Password hash value must be set.");

    /// <summary>
    /// Creates a <see cref="PasswordHash"/> from the specified string.
    /// </summary>
    public static PasswordHash FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current password hash is equal to another password hash.
    /// </summary>
    public bool Equals(PasswordHash other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current password hash is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is PasswordHash other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current password hash.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

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
    /// Returns the string representation of the password hash.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="PasswordHash"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(PasswordHash passwordHash)
        => passwordHash.Value;

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

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Password hash length must be between {MinLength} and {MaxLength} characters.");
        }

        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                throw new ArgumentException(
                    "Password hash cannot contain whitespace.",
                    nameof(value));
            }
        }
    }
}
