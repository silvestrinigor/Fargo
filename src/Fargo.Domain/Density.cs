using UnitsNet.Units;

namespace Fargo.Domain;

/// <summary>
/// Represents a volumetric density value in the domain.
///
/// Wraps <see cref="UnitsNet.Density"/> to enforce non-negativity and provide a
/// consistent domain-level type. The original numeric value and unit are preserved
/// exactly as provided; no implicit unit conversion is performed at construction.
/// </summary>
public readonly struct Density : IEquatable<Density>
{
    private readonly UnitsNet.Density value;

    /// <summary>
    /// Initializes a new instance of <see cref="Density"/>.
    /// </summary>
    /// <param name="value">The numeric magnitude. Must be greater than or equal to zero.</param>
    /// <param name="unit">The unit of measurement.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is negative.
    /// </exception>
    public Density(double value, DensityUnit unit)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Density cannot be negative.");
        }

        this.value = UnitsNet.Density.From(value, unit);
    }

    private Density(UnitsNet.Density density) => value = density;

    /// <summary>
    /// Gets the numeric magnitude in the unit originally used to construct this instance.
    /// </summary>
    public double Value => value.Value;

    /// <summary>
    /// Gets the unit of measurement originally used to construct this instance.
    /// </summary>
    public DensityUnit Unit => value.Unit;

    /// <summary>
    /// Returns the underlying <see cref="UnitsNet.Density"/> for unit conversion or arithmetic.
    /// </summary>
    public UnitsNet.Density ToUnitsNet() => value;

    /// <inheritdoc />
    public bool Equals(Density other) => value.Equals(other.value, UnitsNet.Density.Zero);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Density other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => value.GetHashCode();

    /// <summary>Returns a string representation including value and unit abbreviation.</summary>
    public override string ToString() => value.ToString();

    /// <summary>Determines whether two <see cref="Density"/> instances represent equal physical quantities.</summary>
    public static bool operator ==(Density left, Density right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Density"/> instances represent different physical quantities.</summary>
    public static bool operator !=(Density left, Density right) => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Density"/> to the underlying <see cref="UnitsNet.Density"/>.
    /// </summary>
    public static implicit operator UnitsNet.Density(Density density) => density.value;

    /// <summary>
    /// Explicitly converts a <see cref="UnitsNet.Density"/> to a <see cref="Density"/> domain value object.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public static explicit operator Density(UnitsNet.Density value)
    {
        if (value.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Density cannot be negative.");
        }

        return new Density(value);
    }
}
