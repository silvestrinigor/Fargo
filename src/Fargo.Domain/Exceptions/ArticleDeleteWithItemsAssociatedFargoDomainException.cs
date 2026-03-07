namespace Fargo.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an attempt is made to delete an article
    /// that still has items associated with it.
    /// </summary>
    public sealed class ArticleDeleteWithItemsAssociatedFargoDomainException(Guid articleGuid)
        : FargoDomainException($"Article '{articleGuid}' cannot be deleted because it has associated items.")
    {
        /// <summary>
        /// Gets the identifier of the article that could not be deleted.
        /// </summary>
        public Guid ArticleGuid { get; } = articleGuid;
    }
}