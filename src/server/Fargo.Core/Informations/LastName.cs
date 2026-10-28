using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// Represents a validated last name.
/// </summary>
public readonly struct LastName :
    IEquatable<LastName>,
    IParsable<LastName>
{
    /// <summary>
    /// Maximum last name length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum last name length.
    /// </summary>
    public const int MinLength = 1;

    /// <summary>
    /// Gets the last name string value.
    /// </summary>
    public string Value { get; } = "B";

    /// <summary>
    /// Initializes a last name.
    /// </summary>
    public LastName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Last name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Last name length must be between {MinLength} and {MaxLength} characters.");
        }

        ValidateCharacters(value);

        Value = value;
    }

    /// <summary>
    /// Creates a last name from a string.
    /// </summary>
    public static LastName FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public bool Equals(LastName other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is LastName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two last names are equal.
    /// </summary>
    public static bool operator ==(LastName left, LastName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two last names are different.
    /// </summary>
    public static bool operator !=(LastName left, LastName right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a last name to a string.
    /// </summary>
    public static implicit operator string(LastName lastName)
        => lastName.Value;

    /// <summary>
    /// Converts a string to a last name.
    /// </summary>
    public static explicit operator LastName(string value)
        => new(value);

    /// <inheritdoc />
    public static LastName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out LastName result)
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

            result = new LastName(s);
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
                        "Last name cannot start or end with a separator, or contain consecutive separators.",
                        nameof(value));
                }

                previousWasSeparator = true;
                continue;
            }

            throw new ArgumentException(
                "Last name can contain only letters, spaces, or hyphens.",
                nameof(value));
        }
    }
}
