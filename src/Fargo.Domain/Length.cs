using UnitsNet.Units;

namespace Fargo.Domain;

/// <summary>
/// Represents a physical length value in the domain.
///
/// Wraps <see cref="UnitsNet.Length"/> to enforce non-negativity and provide a
/// consistent domain-level type. The original numeric value and unit are preserved
/// exactly as provided; no implicit unit conversion is performed at construction.
/// </summary>
public readonly struct Length : IEquatable<Length>
{
    private readonly UnitsNet.Length _value;

    /// <summary>
    /// Initializes a new instance of <see cref="Length"/>.
    /// </summary>
    /// <param name="value">The numeric magnitude. Must be greater than or equal to zero.</param>
    /// <param name="unit">The unit of measurement.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is negative.
    /// </exception>
    public Length(double value, LengthUnit unit)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Length cannot be negative.");
        }

        _value = UnitsNet.Length.From(value, unit);
    }

    private Length(UnitsNet.Length length) => _value = length;

    /// <summary>
    /// Gets the numeric magnitude in the unit originally used to construct this instance.
    /// </summary>
    public double Value => _value.Value;

    /// <summary>
    /// Gets the unit of measurement originally used to construct this instance.
    /// </summary>
    public LengthUnit Unit => _value.Unit;

    /// <summary>
    /// Returns the underlying <see cref="UnitsNet.Length"/> for unit conversion or arithmetic.
    /// </summary>
    public UnitsNet.Length ToUnitsNet() => _value;

    /// <inheritdoc />
    public bool Equals(Length other) => _value.Equals(other._value, UnitsNet.Length.Zero);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Length other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _value.GetHashCode();

    /// <summary>Returns a string representation including value and unit abbreviation.</summary>
    public override string ToString() => _value.ToString();

    /// <summary>Determines whether two <see cref="Length"/> instances represent equal physical quantities.</summary>
    public static bool operator ==(Length left, Length right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Length"/> instances represent different physical quantities.</summary>
    public static bool operator !=(Length left, Length right) => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Length"/> to the underlying <see cref="UnitsNet.Length"/>.
    /// </summary>
    public static implicit operator UnitsNet.Length(Length length) => length._value;

    /// <summary>
    /// Explicitly converts a <see cref="UnitsNet.Length"/> to a <see cref="Length"/> domain value object.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public static explicit operator Length(UnitsNet.Length value)
    {
        if (value.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");
        }

        return new Length(value);
    }
}
