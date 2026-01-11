using Fargo.Application.Mediators;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleSingleQuery(
        Guid ArticleGuid,
        DateTime? AtDateTime = null
        ) : IQuery<ArticleReadModel>;

    public sealed class ArticleSingleQueryHandler(IArticleReadRepository repository) : IQueryHandlerAsync<ArticleSingleQuery, ArticleReadModel?>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<ArticleReadModel?> HandleAsync(ArticleSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.ArticleGuid, 
                query.AtDateTime, 
                cancellationToken);
    }
}
