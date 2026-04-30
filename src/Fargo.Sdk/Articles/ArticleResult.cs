namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents an article returned by the API.
/// </summary>
public sealed record ArticleResult
{
    /// <summary>Initializes a new article result.</summary>
    public ArticleResult()
    {
    }

    /// <summary>Initializes a new article result.</summary>
    public ArticleResult(
        Guid Guid,
        string Name,
        string Description,
        ArticleMetrics? Metrics = null,
        TimeSpan? ShelfLife = null,
        bool HasImage = false,
        Guid? EditedByGuid = null,
        ArticleImages? Images = null)
    {
        this.Guid = Guid;
        this.Name = Name;
        this.Description = Description;
        this.Metrics = Metrics;
        this.ShelfLife = ShelfLife;
        this.Images = Images ?? new ArticleImages(HasImage);
        this.EditedByGuid = EditedByGuid;
    }

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; init; }

    /// <summary>The name of the article.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>A short description of the article.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Physical measurements (mass and dimensions), or <see langword="null"/> if unset.</summary>
    public ArticleMetrics? Metrics { get; init; }

    /// <summary>The shelf life of the article, or <see langword="null"/> if unset.</summary>
    public TimeSpan? ShelfLife { get; init; }

    /// <summary>Image state for the article.</summary>
    public ArticleImages Images { get; init; } = new();

    /// <summary>Whether the article has a stored image.</summary>
    public bool HasImage
    {
        get => Images.HasImage;
        init => Images = new ArticleImages(value);
    }

    /// <summary>The identifier of the user who last edited this article.</summary>
    public Guid? EditedByGuid { get; init; }
}
