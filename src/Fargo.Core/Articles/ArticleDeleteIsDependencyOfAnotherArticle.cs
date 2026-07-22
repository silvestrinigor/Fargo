namespace Fargo.Core.Articles;

public sealed class ArticleDeleteIsDependencyOfAnotherArticle(Guid articleGuid)
    : FargoCoreException(
        $"Article '{articleGuid}' cannot be deleted because it is a dependency of another article.",
        FargoCoreErrorType.CannotDeleteArticleThatIsDependencyOfAnotherArticle)
{
    /// <summary>
    /// Gets the identifier of the article that could not be deleted.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}
