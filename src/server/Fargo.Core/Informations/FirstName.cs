using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// Represents a validated first name.
/// </summary>
public readonly struct FirstName : IEquatable<FirstName>, IParsable<FirstName>
{
    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Minimum allowed length for a first name.
    /// </summary>
    public const int MinLength = 1;

    /// <summary>
    /// Gets the first name string value.
    /// </summary>
    public string Value { get; } = "A";

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the first name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, whitespace,
    /// or contains invalid characters or separators.
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

        Value = value;
    }

    /// <summary>
    /// Creates a first name from a string.
    /// </summary>
    public static FirstName FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public bool Equals(FirstName other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two first names are equal.
    /// </summary>
    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two first names are different.
    /// </summary>
    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a first name to a string.
    /// </summary>
    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    /// <summary>
    /// Converts a string to a first name.
    /// </summary>
    public static explicit operator FirstName(string value)
        => new(value);

    /// <inheritdoc />
    public static FirstName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out FirstName result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        s = s.Trim();

        if (s.Length < MinLength || s.Length > MaxLength)
        {
            return false;
        }

        try
        {
            ValidateCharacters(s);

            result = new FirstName(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates the value.
    /// </summary>
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
