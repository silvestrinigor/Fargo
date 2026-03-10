using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    /// <summary>
    /// Query used to retrieve a single article by its unique identifier.
    /// </summary>
    /// <param name="ArticleGuid">
    /// The unique identifier of the article.
    /// </param>
    /// <param name="AsOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the article
    /// as it existed at the specified date and time.
    /// </param>
    public sealed record ArticleSingleQuery(
            Guid ArticleGuid,
            DateTimeOffset? AsOfDateTime = null
            ) : IQuery<ArticleReadModel?>;

    /// <summary>
    /// Handles the execution of <see cref="ArticleSingleQuery"/>.
    /// </summary>
    public sealed class ArticleSingleQueryHandler(
            IArticleQueries repository
            ) : IQueryHandler<ArticleSingleQuery, ArticleReadModel?>
    {
        /// <summary>
        /// Executes the query to retrieve a single article.
        /// </summary>
        /// <param name="query">The query containing the article identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The <see cref="ArticleReadModel"/> if the article exists;
        /// otherwise <c>null</c>.
        /// </returns>
        public async Task<ArticleReadModel?> Handle(
                ArticleSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    query.ArticleGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );
    }
}