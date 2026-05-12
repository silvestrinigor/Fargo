namespace Fargo.Application.Articles;

/// <summary>
/// Exception thrown when an article with the specified identifier cannot be found.
/// </summary>
public class ArticleNotFoundFargoApplicationException(Guid articleGuid)
    : FargoApplicationException($"Article with guid '{articleGuid}' was not found.")
{
    /// <summary>
    /// Gets the identifier of the article that could not be found.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}
