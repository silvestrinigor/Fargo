using UnitsNet.Units;

namespace Fargo.Domain.Articles;

/// <summary>
/// Groups the physical measurement properties of an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// Stored as an owned entity in the <c>Articles</c> table (same columns as before the refactor).
/// <see cref="Density"/> is a computed property derived from <see cref="Mass"/> and the three
/// length dimensions; it is never persisted.
/// </remarks>
public sealed class ArticleMetrics
{
    /// <summary>Gets or sets the physical mass of the article.</summary>
    public Mass? Mass { get; set; }

    /// <summary>Gets or sets the X dimension of the article.</summary>
    public Length? LengthX { get; set; }

    /// <summary>Gets or sets the Y dimension of the article.</summary>
    public Length? LengthY { get; set; }

    /// <summary>Gets or sets the Z dimension of the article.</summary>
    public Length? LengthZ { get; set; }

    /// <summary>
    /// Gets the volumetric density computed from <see cref="Mass"/> and the three length dimensions,
    /// expressed in the natural unit for the given mass and length unit combination.
    /// Returns <see langword="null"/> when any measurement is absent or any dimension is zero or negative.
    /// </summary>
    public Density? Density
    {
        get
        {
            if (Mass is not { } m || LengthX is not { } x || LengthY is not { } y || LengthZ is not { } z)
            {
                return null;
            }

            var xNet = x.ToUnitsNet();
            var yNet = y.ToUnitsNet();
            var zNet = z.ToUnitsNet();

            if (xNet.Meters <= 0 || yNet.Meters <= 0 || zNet.Meters <= 0)
            {
                return null;
            }

            var densityKgPerM3 = m.ToUnitsNet().Kilograms / (xNet.Meters * yNet.Meters * zNet.Meters);
            var naturalUnit = GetNaturalUnit(m.Unit, x.Unit);
            var converted = UnitsNet.Density.FromKilogramsPerCubicMeter(densityKgPerM3).ToUnit(naturalUnit);
            return (Density)converted;
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
