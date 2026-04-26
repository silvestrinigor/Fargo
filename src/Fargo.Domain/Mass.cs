using UnitsNet.Units;

namespace Fargo.Domain;

/// <summary>
/// Represents a physical mass value in the domain.
///
/// Wraps <see cref="UnitsNet.Mass"/> to enforce non-negativity and provide a
/// consistent domain-level type. The original numeric value and unit are preserved
/// exactly as provided; no implicit unit conversion is performed at construction.
/// </summary>
public readonly struct Mass : IEquatable<Mass>
{
    private readonly UnitsNet.Mass value;

    /// <summary>
    /// Initializes a new instance of <see cref="Mass"/>.
    /// </summary>
    /// <param name="value">The numeric magnitude. Must be greater than or equal to zero.</param>
    /// <param name="unit">The unit of measurement.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is negative.
    /// </exception>
    public Mass(double value, MassUnit unit)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Mass cannot be negative.");
        }

        this.value = UnitsNet.Mass.From(value, unit);
    }

    private Mass(UnitsNet.Mass mass) => value = mass;

    /// <summary>
    /// Gets the numeric magnitude in the unit originally used to construct this instance.
    /// </summary>
    public double Value => value.Value;

    /// <summary>
    /// Gets the unit of measurement originally used to construct this instance.
    /// </summary>
    public MassUnit Unit => value.Unit;

    /// <summary>
    /// Returns the underlying <see cref="UnitsNet.Mass"/> for unit conversion or arithmetic.
    /// </summary>
    public UnitsNet.Mass ToUnitsNet() => value;

    /// <inheritdoc />
    public bool Equals(Mass other) => value.Equals(other.value, UnitsNet.Mass.Zero);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Mass other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => value.GetHashCode();

    /// <summary>Returns a string representation including value and unit abbreviation.</summary>
    public override string ToString() => value.ToString();

    /// <summary>Determines whether two <see cref="Mass"/> instances represent equal physical quantities.</summary>
    public static bool operator ==(Mass left, Mass right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Mass"/> instances represent different physical quantities.</summary>
    public static bool operator !=(Mass left, Mass right) => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Mass"/> to the underlying <see cref="UnitsNet.Mass"/>.
    /// </summary>
    public static implicit operator UnitsNet.Mass(Mass mass) => mass.value;

    /// <summary>
    /// Explicitly converts a <see cref="UnitsNet.Mass"/> to a <see cref="Mass"/> domain value object.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public static explicit operator Mass(UnitsNet.Mass value)
    {
        if (value.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Mass cannot be negative.");
        }

        return new Mass(value);
    }
}
