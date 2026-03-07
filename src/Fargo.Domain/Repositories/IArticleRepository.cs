using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="Article"/> entities.
    ///
    /// This repository provides access to article persistence operations and
    /// domain-related queries involving articles.
    /// </summary>
    public interface IArticleRepository
    {
        /// <summary>
        /// Gets an article by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the article.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="Article"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<Article?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Determines whether the specified article has any associated items.
        /// </summary>
        /// <param name="articleGuid">The unique identifier of the article.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the article has associated items; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> HasItemsAssociated(
                Guid articleGuid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Adds a new article to the persistence context.
        /// </summary>
        /// <param name="article">The article to add.</param>
        void Add(Article article);

        /// <summary>
        /// Removes an article from the persistence context.
        /// </summary>
        /// <param name="article">The article to remove.</param>
        void Remove(Article article);
    }
}