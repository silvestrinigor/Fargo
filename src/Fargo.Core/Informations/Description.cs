using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// Represents a validated textual description.
/// </summary>
public readonly struct Description : IEquatable<Description>, IParsable<Description>
{
    /// <summary>
    /// Description maximum length.
    /// </summary>
    public const int MaxLength = 500;

    /// <summary>
    /// Gets string value of the description.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Description"/> value object.
    /// </summary>
    /// <param name="value">The textual description.</param>
    public Description(string value)
    {
        Validate(value);
        Value = value;
    }

    /// <summary>
    /// Gets an empty description.
    /// </summary>
    public static Description Empty => new(string.Empty);

    /// <summary>
    /// Creates a new <see cref="Description"/> from the specified string.
    /// </summary>
    public static Description FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public bool Equals(Description other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Description other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two descriptions are equal.
    /// </summary>
    public static bool operator ==(Description left, Description right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two descriptions are different.
    /// </summary>
    public static bool operator !=(Description left, Description right)
        => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(Description description)
        => description.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="Description"/>.
    /// </summary>
    public static explicit operator Description(string value)
        => new(value);

    /// <inheritdoc />
    public static Description Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Description result)
    {
        try
        {
            result = new Description(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Validates the specified description value.
    /// </summary>
    private static void Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Description length must be shorter or equal to {MaxLength} characters.");
        }
    }
}
