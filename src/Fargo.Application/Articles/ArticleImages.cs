namespace Fargo.Application.Articles;

/// <summary>
/// Describes the image state of an article without exposing storage-provider details.
/// </summary>
public sealed class ArticleImages
{
    /// <summary>Initializes an empty image state.</summary>
    public ArticleImages()
    {
    }

    /// <summary>Initializes the image state.</summary>
    /// <param name="hasImage">Whether the article has a stored image.</param>
    public ArticleImages(bool hasImage)
    {
        HasImage = hasImage;
    }

    /// <summary>Gets whether the article has a stored image.</summary>
    public bool HasImage { get; init; }
}
