using Fargo.Application.Common;
using Fargo.Application.Models.ArticleModels;

namespace Fargo.HttpApi.Client.Interfaces
{
    /// <summary>
    /// Defines the contract for article-related HTTP API operations.
    /// </summary>
    public interface IArticleClient
    {
        /// <summary>
        /// Gets a single article by its identifier.
        /// </summary>
        Task<ArticleReadModel?> GetSingleAsync(
            Guid articleGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets multiple articles with optional pagination and temporal query.
        /// </summary>
        Task<IReadOnlyCollection<ArticleReadModel>> GetManyAsync(
            DateTimeOffset? temporalAsOf = null,
            Page? page = null,
            Limit? limit = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new article and returns its identifier.
        /// </summary>
        Task<Guid> CreateAsync(
            ArticleCreateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing article.
        /// </summary>
        Task UpdateAsync(
            Guid articleGuid,
            ArticleUpdateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an article.
        /// </summary>
        Task DeleteAsync(
            Guid articleGuid,
            CancellationToken cancellationToken = default);
    }
}