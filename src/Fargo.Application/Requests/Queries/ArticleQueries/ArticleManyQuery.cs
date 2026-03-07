using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    /// <summary>
    /// Query used to retrieve multiple articles.
    /// </summary>
    /// <param name="AsOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the articles
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Pagination parameters used to limit and offset the result set.
    /// </param>
    public sealed record ArticleManyQuery(
            DateTime? AsOfDateTime = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<ArticleReadModel>>;

    /// <summary>
    /// Handles the execution of <see cref="ArticleManyQuery"/>.
    /// </summary>
    public sealed class ArticleManyQueryHandler(
            IArticleReadRepository articleRepository
            )
        : IQueryHandler<ArticleManyQuery, IEnumerable<ArticleReadModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple articles.
        /// </summary>
        /// <param name="query">The query containing the filtering and pagination parameters.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// A collection of <see cref="ArticleReadModel"/> representing the retrieved articles.
        /// </returns>
        public async Task<IEnumerable<ArticleReadModel>> Handle(
                ArticleManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            return await articleRepository.GetMany(
                    query.AsOfDateTime,
                    query.Pagination,
                    cancellationToken
                    );
        }
    }
}