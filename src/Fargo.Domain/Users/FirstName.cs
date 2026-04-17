namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents a validated first name value object used in the domain.
///
/// This value object guarantees that a first name:
/// - is not null, empty, or whitespace
/// - is within the allowed length range
/// - contains only letters, spaces, or hyphens
/// - does not start or end with spaces or hyphens
/// - does not contain consecutive spaces or hyphens
/// </summary>
public readonly struct FirstName : IEquatable<FirstName>
{
    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Minimum allowed length for a first name.
    /// </summary>
    public const int MinLength = 2;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the first name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public FirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "First name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"First name length must be between {MinLength} and {MaxLength} characters.");
        }

        ValidateCharacters(value);

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the first name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("First name not initialized.");

    /// <summary>
    /// Creates a new <see cref="FirstName"/> from a string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName FromString(string value)
        => new(value);

    /// <summary>
    /// Creates a new validated <see cref="FirstName"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="FirstName"/> instance.</returns>
    public static FirstName NewFirstName(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current first name is equal to another first name.
    /// </summary>
    /// <param name="other">The other first name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both first names have the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(FirstName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current first name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current first name.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="FirstName"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current first name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are equal.
    /// </summary>
    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are different.
    /// </summary>
    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the first name.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="FirstName"/> to its string representation.
    /// </summary>
    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="FirstName"/>.
    /// </summary>
    public static explicit operator FirstName(string value)
        => new(value);

    private static void ValidateCharacters(string value)
    {
        var previousWasSeparator = false;

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            var isSeparator = current == ' ' || current == '-';

            if (char.IsLetter(current))
            {
                previousWasSeparator = false;
                continue;
            }

            if (isSeparator)
            {
                if (i == 0 || i == value.Length - 1 || previousWasSeparator)
                {
                    throw new ArgumentException(
                        "First name cannot start or end with a separator, or contain consecutive separators.",
                        nameof(value));
                }

                previousWasSeparator = true;
                continue;
            }

            throw new ArgumentException(
                "First name can contain only letters, spaces, or hyphens.",
                nameof(value));
        }
    }
}
