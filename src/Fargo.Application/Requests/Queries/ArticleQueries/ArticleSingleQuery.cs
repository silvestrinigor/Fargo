using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleSingleQuery(
        Guid ArticleGuid,
        DateTime? AsOfDateTime = null
        ) : IQuery<Task<ArticleReadModel?>>;

    public sealed class ArticleSingleQueryHandler(IArticleReadRepository repository) : IQueryHandler<ArticleSingleQuery, Task<ArticleReadModel?>>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<ArticleReadModel?> Handle(ArticleSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.ArticleGuid,
                query.AsOfDateTime,
                cancellationToken);
    }
}