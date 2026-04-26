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
    /// Gets the volumetric density in kg/m³, computed from <see cref="Mass"/> and the three
    /// length dimensions. Returns <see langword="null"/> when any measurement is absent or any
    /// dimension is zero.
    /// </summary>
    public double? Density => (Mass, LengthX, LengthY, LengthZ) switch
    {
        ({ } m, { } x, { } y, { } z)
            when x.ToUnitsNet().Meters > 0 && y.ToUnitsNet().Meters > 0 && z.ToUnitsNet().Meters > 0
            => m.ToUnitsNet().Kilograms
               / (x.ToUnitsNet().Meters * y.ToUnitsNet().Meters * z.ToUnitsNet().Meters),
        _ => null
    };
}
