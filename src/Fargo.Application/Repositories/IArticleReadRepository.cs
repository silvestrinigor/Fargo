using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;

namespace Fargo.Application.Repositories
{
    /// <summary>
    /// Provides read operations for <see cref="ArticleReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This repository is part of the query side of the application (CQRS)
    /// and is responsible only for retrieving article data.
    /// It must not modify the state of the system.
    /// </remarks>
    public interface IArticleReadRepository
    {
        /// <summary>
        /// Retrieves a single article by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the article.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the article
        /// as it existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="ArticleReadModel"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<ArticleReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Retrieves multiple articles using pagination.
        /// </summary>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the articles
        /// as they existed at the specified date and time.
        /// </param>
        /// <param name="pagination">
        /// Pagination parameters used to limit and offset the result set.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="ArticleReadModel"/>.
        /// </returns>
        Task<IReadOnlyCollection<ArticleReadModel>> GetMany(
                DateTimeOffset? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                );
    }
}