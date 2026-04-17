namespace Fargo.Domain;

/// <summary>
/// Represents a validated name value object used across the domain.
///
/// This value object guarantees that a name is always within the
/// allowed length range and is not null, empty, or composed only of whitespace.
/// </summary>
public readonly struct Name : IEquatable<Name>
{
    /// <summary>
    /// Maximum allowed length for a name.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum allowed length for a name.
    /// </summary>
    public const int MinLength = 3;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Name"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name length must be between {MinLength} and {MaxLength} characters."
            );
        }

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Name not initialized.");

    /// <summary>
    /// Creates a new <see cref="Name"/> from a string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A validated <see cref="Name"/> instance.</returns>
    public static Name FromString(string value)
        => new(value);

    /// <summary>
    /// Creates a new validated <see cref="Name"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="Name"/> instance.</returns>
    public static Name NewName(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current name is equal to another name.
    /// </summary>
    /// <param name="other">The other name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names have the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Name other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current name.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="Name"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are different.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Name"/> to its string representation.
    /// </summary>
    public static implicit operator string(Name name)
        => name.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="Name"/>.
    /// </summary>
    public static explicit operator Name(string value)
        => new(value);
}
