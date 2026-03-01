using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleSingleQuery(
            Guid ArticleGuid,
            DateTime? AsOfDateTime = null
            ) : IQuery<ArticleReadModel?>;

    public sealed class ArticleSingleQueryHandler(
            IArticleReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<ArticleSingleQuery, ArticleReadModel?>
    {
        public async Task<ArticleReadModel?> Handle(
                ArticleSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    query.ArticleGuid,
                    [],
                    query.AsOfDateTime,
                    cancellationToken
                    );
    }
}