using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Lightweight projection of an <see cref="Article"/> used for read operations.
/// </summary>
public sealed record ArticleInformation
{
    /// <summary>Initializes a new article information projection.</summary>
    public ArticleInformation(
        Guid Guid,
        Name Name,
        Description Description,
        ArticleMetrics? Metrics = null,
        TimeSpan? ShelfLife = null,
        bool HasImage = false,
        Guid? EditedByGuid = null,
        ArticleImages? Images = null,
        ArticleBarcodes? Barcodes = null)
    {
        this.Guid = Guid;
        this.Name = Name;
        this.Description = Description;
        this.Metrics = Metrics;
        this.ShelfLife = ShelfLife;
        this.Images = Images ?? new ArticleImages(HasImage);
        this.Barcodes = Barcodes ?? new ArticleBarcodes();
        this.EditedByGuid = EditedByGuid;
    }

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; init; }

    /// <summary>The name of the article.</summary>
    public Name Name { get; init; }

    /// <summary>The description of the article.</summary>
    public Description Description { get; init; }

    /// <summary>Physical measurements (mass and dimensions). May be <see langword="null"/> when no measurements have been set.</summary>
    public ArticleMetrics? Metrics { get; init; }

    /// <summary>The shelf life of the article, or <see langword="null"/> if unset.</summary>
    public TimeSpan? ShelfLife { get; init; }

    /// <summary>Image state for the article.</summary>
    public ArticleImages Images { get; init; }

    /// <summary>Barcode state for the article, grouped by barcode format.</summary>
    public ArticleBarcodes Barcodes { get; init; }

    /// <summary>Whether the article has a stored image.</summary>
    public bool HasImage
    {
        get => Images.HasImage;
        init => Images = new ArticleImages(value);
    }

    /// <summary>The identifier of the user who last edited this article.</summary>
    public Guid? EditedByGuid { get; init; }
}
