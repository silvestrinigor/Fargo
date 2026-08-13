namespace Fargo.Core.Articles;

/// <summary>
/// Represents the classification of an article within the inventory system.
/// </summary>
public enum ArticleType : byte
{
    /// <summary>
    /// Represents a standard article.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Represents a variation of another article.
    /// </summary>
    Variation = 2,

    /// <summary>
    /// Represents an article composed of multiple units of another article.
    /// </summary>
    Pack = 3,

    /// <summary>
    /// Represents an article consisting of a predefined set of related articles.
    /// </summary>
    Kit = 4,

    /// <summary>
    /// Represents an article that functions as a container for storing or transporting other articles.
    /// </summary>
    Container = 5
}
