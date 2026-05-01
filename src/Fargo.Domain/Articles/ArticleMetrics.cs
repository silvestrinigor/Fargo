using UnitsNet;

namespace Fargo.Domain.Articles;

/// <summary>
/// Groups the physical measurement properties of an <see cref="Article"/>.
/// </summary>
public sealed class ArticleMetrics
{
    /// <summary>
    /// Gets or sets the physical mass of the article.
    /// </summary>
    public Mass? Mass { get; set; }

    /// <summary>
    /// Gets or sets the X dimension of the article.
    /// </summary>
    public Length? LengthX { get; set; }

    /// <summary>
    /// Gets or sets the Y dimension of the article.
    /// </summary>
    public Length? LengthY { get; set; }

    /// <summary>
    /// Gets or sets the Z dimension of the article.
    /// </summary>
    public Length? LengthZ { get; set; }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => LengthX * LengthY * LengthZ;

    /// <summary>
    /// Gets the density of the article.
    /// </summary>
    public Density? Density => Mass / Volume;
}
