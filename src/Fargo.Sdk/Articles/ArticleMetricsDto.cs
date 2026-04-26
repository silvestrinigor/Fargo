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
    /// Gets the volumetric density in kg/m³, computed from <see cref="Mass"/> and the three
    /// length dimensions. Returns <see langword="null"/> when any measurement is absent or any
    /// dimension is zero.
    /// </summary>
    public DensityDto? Density => ComputeDensity(Mass, LengthX, LengthY, LengthZ);

    private static DensityDto? ComputeDensity(MassDto? m, LengthDto? x, LengthDto? y, LengthDto? z)
    {
        if (m is null || x is null || y is null || z is null)
        {
            return null;
        }

        var massKg = UnitsNet.Mass.Parse($"{m.Value} {m.Unit}").Kilograms;
        var xM = UnitsNet.Length.Parse($"{x.Value} {x.Unit}").Meters;
        var yM = UnitsNet.Length.Parse($"{y.Value} {y.Unit}").Meters;
        var zM = UnitsNet.Length.Parse($"{z.Value} {z.Unit}").Meters;
        var vol = xM * yM * zM;

        return vol > 0 ? new DensityDto(massKg / vol, "kg/m³") : null;
    }
}
