namespace Fargo.Core.Articles;

/// <summary>
/// Defines an article variation relationship.
/// </summary>
/// <remarks>
/// A variation article is derived from another article, usually representing
/// a different version, option, or presentation of the original article.
/// </remarks>
public sealed class ArticleVariation
{
    private ArticleVariation()
    {
    }

    internal ArticleVariation(Article fromArticle)
    {
        FromArticle = fromArticle;
    }

    /// <summary>
    /// Gets the unique identifier of the source article.
    /// </summary>
    public Guid FromArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article from which this variation originates.
    /// </summary>
    public Article FromArticle
    {
        get;
        private init
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    } = null!;
}
