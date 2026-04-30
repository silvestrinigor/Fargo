namespace Fargo.Sdk.Articles;

/// <summary>
/// Groups the image state of an article as returned by the API.
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
