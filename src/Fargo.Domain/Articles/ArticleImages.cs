namespace Fargo.Domain.Articles;

/// <summary>
/// Groups the image properties of an <see cref="Article"/>.
/// </summary>
public sealed class ArticleImages
{
    /// <summary>
    /// Gets or sets the storage key of the article's image.
    /// When <see langword="null"/>, the article has no image.
    /// </summary>
    public string? ImageKey { get; set; }

    /// <summary>Gets whether the article has a stored image.</summary>
    public bool HasImage => ImageKey is not null;
}
