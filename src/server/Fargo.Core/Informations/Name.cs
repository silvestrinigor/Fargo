using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// Represents a validated name.
/// </summary>
public readonly struct Name : IEquatable<Name>, IParsable<Name>
{
    /// <summary>
    /// Maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum length.
    /// </summary>
    public const int MinLength = 1;

    /// <summary>
    /// Gets the name string value.
    /// </summary>
    public string Value { get; } = "C";

    /// <summary>
    /// Initializes a name.
    /// </summary>
    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name length must be between {MinLength} and {MaxLength} characters.");
        }

        Value = value;
    }

    /// <summary>
    /// Creates a name from a string.
    /// </summary>
    public static Name FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public bool Equals(Name other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two names are equal.
    /// </summary>
    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two names are different.
    /// </summary>
    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a name to a string.
    /// </summary>
    public static implicit operator string(Name name)
        => name.Value;

    /// <summary>
    /// Converts a string to a name.
    /// </summary>
    public static explicit operator Name(string value)
        => new(value);

    /// <inheritdoc />
    public static Name Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Name result)
    {
        try
        {
            result = new Name(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
