using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Domain.Articles;

/// <summary>
/// Groups the physical measurement properties of an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// Stored as an owned entity in the <c>Articles</c> table.
/// <see cref="Density"/> is a computed property derived from <see cref="Mass"/> and the three
/// length dimensions; it is never persisted.
/// </remarks>
public sealed class ArticleMetrics
{
    private Mass? _mass;
    private Length? _lengthX;
    private Length? _lengthY;
    private Length? _lengthZ;

    /// <summary>Gets or sets the physical mass of the article.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public Mass? Mass
    {
        get => _mass;
        set
        {
            if (value.HasValue && value.Value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Mass cannot be negative.");
            _mass = value;
        }
    }

    /// <summary>Gets or sets the X dimension of the article.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public Length? LengthX
    {
        get => _lengthX;
        set
        {
            if (value.HasValue && value.Value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");
            _lengthX = value;
        }
    }

    /// <summary>Gets or sets the Y dimension of the article.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public Length? LengthY
    {
        get => _lengthY;
        set
        {
            if (value.HasValue && value.Value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");
            _lengthY = value;
        }
    }

    /// <summary>Gets or sets the Z dimension of the article.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public Length? LengthZ
    {
        get => _lengthZ;
        set
        {
            if (value.HasValue && value.Value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");
            _lengthZ = value;
        }
    }

    /// <summary>
    /// Gets the volumetric density computed from <see cref="Mass"/> and the three length dimensions,
    /// expressed in the natural unit for the given mass and length unit combination.
    /// Returns <see langword="null"/> when any measurement is absent or any dimension is zero or negative.
    /// </summary>
    public Density? Density
    {
        get
        {
            if (_mass is not { } m || _lengthX is not { } x || _lengthY is not { } y || _lengthZ is not { } z)
                return null;

            if (x.Meters <= 0 || y.Meters <= 0 || z.Meters <= 0)
                return null;

            var densityKgPerM3 = m.Kilograms / (x.Meters * y.Meters * z.Meters);
            var naturalUnit = GetNaturalUnit(m.Unit, x.Unit);
            return UnitsNet.Density.FromKilogramsPerCubicMeter(densityKgPerM3).ToUnit(naturalUnit);
        }
    }

    private static DensityUnit GetNaturalUnit(MassUnit massUnit, LengthUnit lengthUnit)
        => (massUnit, lengthUnit) switch
        {
            (MassUnit.Kilogram, LengthUnit.Meter) => DensityUnit.KilogramPerCubicMeter,
            (MassUnit.Kilogram, LengthUnit.Centimeter) => DensityUnit.KilogramPerCubicCentimeter,
            (MassUnit.Kilogram, LengthUnit.Millimeter) => DensityUnit.KilogramPerCubicMillimeter,
            (MassUnit.Gram, LengthUnit.Meter) => DensityUnit.GramPerCubicMeter,
            (MassUnit.Gram, LengthUnit.Centimeter) => DensityUnit.GramPerCubicCentimeter,
            (MassUnit.Gram, LengthUnit.Millimeter) => DensityUnit.GramPerCubicMillimeter,
            (MassUnit.Pound, LengthUnit.Foot) => DensityUnit.PoundPerCubicFoot,
            (MassUnit.Pound, LengthUnit.Inch) => DensityUnit.PoundPerCubicInch,
            _ => DensityUnit.KilogramPerCubicMeter
        };
}
