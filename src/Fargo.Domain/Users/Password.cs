namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents a validated plaintext password in the domain.
///
/// This value object enforces minimum security rules for passwords
/// before they are hashed and stored in the system.
/// </summary>
public readonly struct Password : IEquatable<Password>
{
    /// <summary>
    /// Maximum allowed length for the password.
    /// </summary>
    public const int MaxLength = 512;

    /// <summary>
    /// Minimum allowed length for the password.
    /// </summary>
    public const int MinLength = 9;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Password"/> value object.
    /// </summary>
    /// <param name="value">The plaintext password.</param>
    public Password(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying password string.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the password was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Password value must be set.");

    /// <summary>
    /// Creates a new <see cref="Password"/> from a string.
    /// </summary>
    public static Password FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current password is equal to another password.
    /// </summary>
    public bool Equals(Password other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current password is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Password other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current password.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Password"/> instances are equal.
    /// </summary>
    public static bool operator ==(Password left, Password right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Password"/> instances are different.
    /// </summary>
    public static bool operator !=(Password left, Password right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the password.
    /// </summary>
    /// <remarks>
    /// Use carefully, as this exposes the plaintext password.
    /// </remarks>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Password"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(Password password)
        => password.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="Password"/>.
    /// </summary>
    public static explicit operator Password(string value)
        => new(value);

    /// <summary>
    /// Validates the password value.
    /// </summary>
    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Password cannot be null, empty, or whitespace.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Password length must be between {MinLength} and {MaxLength} characters.");
        }

        if (value.Contains(' '))
        {
            throw new ArgumentException(
                "Password cannot contain spaces.",
                nameof(value));
        }

        bool hasLetter = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (var c in value)
        {
            if (char.IsLetter(c))
            {
                hasLetter = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else
            {
                hasSpecial = true;
            }
        }

        if (!hasLetter)
        {
            throw new ArgumentException(
                "Password must contain at least one letter.",
                nameof(value));
        }

        if (!hasDigit)
        {
            throw new ArgumentException(
                "Password must contain at least one digit.",
                nameof(value));
        }

        if (!hasSpecial)
        {
            throw new ArgumentException(
                "Password must contain at least one special character.",
                nameof(value));
        }
    }
}
