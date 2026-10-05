namespace Fargo.Core.Articles;

/// <summary>
/// Represents the relationship between an article variation and the article
/// from which it was created.
/// </summary>
/// <remarks>
/// A variation article represents a distinct article derived from another
/// article while maintaining a relationship with the original article.
/// </remarks>
public sealed class ArticleVariation
{
    /// <summary>
    /// Gets the unique identifier of the variation article.
    /// </summary>
    public Guid VariationArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article representing this variation.
    /// </summary>
    public Article VariationArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the article from which the variation
    /// was created.
    /// </summary>
    public Guid FromArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article from which this variation was created.
    /// </summary>
    public Article FromArticle { get; private init; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleVariation"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is intended for use by Entity Framework.
    /// </remarks>
    private ArticleVariation()
    {
    }

    /// <summary>
    /// Initializes a new article variation relationship.
    /// </summary>
    /// <param name="fromArticle">
    /// The article from which the variation is created.
    /// </param>
    /// <param name="variationArticle">
    /// The article representing the new variation.
    /// </param>
    internal ArticleVariation(Article fromArticle, Article variationArticle)
    {
        ArgumentNullException.ThrowIfNull(fromArticle);
        ArgumentNullException.ThrowIfNull(variationArticle);

        FromArticle = fromArticle;
        FromArticleGuid = fromArticle.Guid;

        VariationArticle = variationArticle;
        VariationArticleGuid = variationArticle.Guid;
    }
}
