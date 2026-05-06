namespace Fargo.Sdk.Articles;

/// <summary>
/// Groups the physical measurement properties of an article as returned by the API.
/// <see cref="Density"/> is computed client-side from the other measurements and is never
/// sent to or received from the server.
/// </summary>
public sealed class ArticleMetrics
{
    /// <summary>Gets or sets the physical mass of the article.</summary>
    public Mass? Mass { get; init; }

    /// <summary>Gets or sets the X dimension of the article.</summary>
    public Length? LengthX { get; init; }

    /// <summary>Gets or sets the Y dimension of the article.</summary>
    public Length? LengthY { get; init; }

    /// <summary>Gets or sets the Z dimension of the article.</summary>
    public Length? LengthZ { get; init; }

    /// <summary>
    /// Gets the volumetric density computed from <see cref="Mass"/> and the three length dimensions,
    /// expressed in the natural unit for the given mass and length unit combination.
    /// Returns <see langword="null"/> when any measurement is absent or any dimension is zero or negative.
    /// </summary>
    public Density? Density => ComputeDensity(Mass, LengthX, LengthY, LengthZ);

    private static Density? ComputeDensity(Mass? m, Length? x, Length? y, Length? z)
    {
        if (m is null || x is null || y is null || z is null)
        {
            return null;
        }

        var xM = ToMeters(x.Value, x.Unit);
        var yM = ToMeters(y.Value, y.Unit);
        var zM = ToMeters(z.Value, z.Unit);

        if (xM <= 0 || yM <= 0 || zM <= 0)
        {
            return null;
        }

        var massKg = ToKilograms(m.Value, m.Unit);
        var densityKgPerM3 = massKg / (xM * yM * zM);
        var naturalUnit = GetNaturalUnit(m.Unit, x.Unit);
        var densityValue = FromKilogramsPerCubicMeter(densityKgPerM3, naturalUnit);

        return new Density(densityValue, naturalUnit);
    }

    private static double ToKilograms(double value, MassUnit unit) => unit switch
    {
        MassUnit.Kilogram => value,
        MassUnit.Gram => value / 1_000.0,
        MassUnit.Milligram => value / 1_000_000.0,
        MassUnit.Pound => value * 0.45359237,
        MassUnit.Ounce => value * 0.028349523125,
        _ => value
    };

    private static double ToMeters(double value, LengthUnit unit) => unit switch
    {
        LengthUnit.Meter => value,
        LengthUnit.Centimeter => value / 100.0,
        LengthUnit.Millimeter => value / 1_000.0,
        LengthUnit.Kilometer => value * 1_000.0,
        LengthUnit.Inch => value * 0.0254,
        LengthUnit.Foot => value * 0.3048,
        _ => value
    };

    private static double FromKilogramsPerCubicMeter(double kgPerM3, DensityUnit unit) => unit switch
    {
        DensityUnit.KilogramPerCubicMeter => kgPerM3,
        DensityUnit.KilogramPerCubicCentimeter => kgPerM3 / 1_000_000.0,
        DensityUnit.KilogramPerCubicMillimeter => kgPerM3 / 1_000_000_000.0,
        DensityUnit.GramPerCubicMeter => kgPerM3 * 1_000.0,
        DensityUnit.GramPerCubicCentimeter => kgPerM3 / 1_000.0,
        DensityUnit.GramPerCubicMillimeter => kgPerM3 / 1_000_000.0,
        DensityUnit.PoundPerCubicFoot => kgPerM3 * 0.062427960576145,
        DensityUnit.PoundPerCubicInch => kgPerM3 * 0.000036127292,
        _ => kgPerM3
    };

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
