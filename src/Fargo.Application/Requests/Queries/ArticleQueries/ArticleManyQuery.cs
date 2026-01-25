using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleManyQuery(
        DateTime? AsOfDateTime = null,
        Pagination Pagination = default
        ) : IQuery<Task<IEnumerable<ArticleReadModel>>>;

    public sealed class ArticleManyQueryHandler(IArticleReadRepository repository) : IQueryHandler<ArticleManyQuery, Task<IEnumerable<ArticleReadModel>>>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<IEnumerable<ArticleReadModel>> Handle(ArticleManyQuery query, CancellationToken cancellationToken = default)
            => await repository.GetManyAsync(
                query.AsOfDateTime,
                query.Pagination,
                cancellationToken);
    }
}
