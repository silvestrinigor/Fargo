using UnitsNet;
using UnitsNet.Units;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Groups the physical measurement properties of an article as returned by the API.
/// <see cref="Density"/> is computed client-side from the other measurements and is never
/// sent to or received from the server.
/// </summary>
public sealed class ArticleMetricsDto
{
    /// <summary>Gets or sets the physical mass of the article.</summary>
    public MassDto? Mass { get; init; }

    /// <summary>Gets or sets the X dimension of the article.</summary>
    public LengthDto? LengthX { get; init; }

    /// <summary>Gets or sets the Y dimension of the article.</summary>
    public LengthDto? LengthY { get; init; }

    /// <summary>Gets or sets the Z dimension of the article.</summary>
    public LengthDto? LengthZ { get; init; }

    /// <summary>
    /// Gets the volumetric density computed from <see cref="Mass"/> and the three length dimensions,
    /// expressed in the natural unit for the given mass and length unit combination.
    /// Returns <see langword="null"/> when any measurement is absent or any dimension is zero or negative.
    /// </summary>
    public DensityDto? Density => ComputeDensity(Mass, LengthX, LengthY, LengthZ);

    private static DensityDto? ComputeDensity(MassDto? m, LengthDto? x, LengthDto? y, LengthDto? z)
    {
        if (m is null || x is null || y is null || z is null)
        {
            return null;
        }

        var massUnit = UnitParser.Default.Parse<MassUnit>(m.Unit);
        var lxUnit = UnitParser.Default.Parse<LengthUnit>(x.Unit);
        var lyUnit = UnitParser.Default.Parse<LengthUnit>(y.Unit);
        var lzUnit = UnitParser.Default.Parse<LengthUnit>(z.Unit);

        var mass = UnitsNet.Mass.From(m.Value, massUnit);
        var lx = UnitsNet.Length.From(x.Value, lxUnit);
        var ly = UnitsNet.Length.From(y.Value, lyUnit);
        var lz = UnitsNet.Length.From(z.Value, lzUnit);

        if (lx.Meters <= 0 || ly.Meters <= 0 || lz.Meters <= 0)
        {
            return null;
        }

        var densityKgPerM3 = mass.Kilograms / (lx.Meters * ly.Meters * lz.Meters);
        var naturalUnit = GetNaturalUnit(massUnit, lxUnit);
        var density = UnitsNet.Density.FromKilogramsPerCubicMeter(densityKgPerM3).ToUnit(naturalUnit);

        return new DensityDto(density.Value, UnitsNet.Density.GetAbbreviation(naturalUnit));
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
