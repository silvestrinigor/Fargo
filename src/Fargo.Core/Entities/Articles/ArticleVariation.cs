namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Links a variation article to the article it was derived from.
/// </summary>
public sealed class ArticleVariation
{
    public Guid VariationArticleGuid { get; private init; }

    public Article VariationArticle { get; private init; } = null!;

    public Guid FromArticleGuid { get; private init; }

    public Article FromArticle { get; private init; } = null!;

    // Required by EF Core.
    private ArticleVariation()
    {
    }

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
