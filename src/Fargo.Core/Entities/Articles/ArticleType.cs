namespace Fargo.Core.Entities.Articles;

public enum ArticleType : byte
{
    Default = 1,

    Variation = 2,

    /// <summary>
    /// Composed of multiple units of another article.
    /// </summary>
    Pack = 3,

    /// <summary>
    /// A predefined set of other articles.
    /// </summary>
    Kit = 4,

    Container = 5
}
