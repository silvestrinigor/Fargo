using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleManyQuery(
            DateTime? AsOfDateTime = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<ArticleReadModel>>;

    public sealed class ArticleManyQueryHandler(
            IArticleReadRepository repository,
            ICurrentUser currentUser
            )
        : IQueryHandler<ArticleManyQuery, IEnumerable<ArticleReadModel>>
    {
        public async Task<IEnumerable<ArticleReadModel>> Handle(
                ArticleManyQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetManyAsync(
                    currentUser.PartitionGuids,
                    query.AsOfDateTime,
                    query.Pagination,
                    cancellationToken
                    );
    }
}