using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories.ArticleRepositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleAllQuery(
        DateTime? AtDateTime,
        Pagination Pagination
        ) : IQuery<IEnumerable<ArticleDto>>;

    public sealed class ArticleAllQueryHandler(IArticleReadRepository repository) : IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<IEnumerable<ArticleDto>> HandleAsync(ArticleAllQuery query, CancellationToken cancellationToken = default)
        {
            var articles = await repository.GetManyAsync(
                query.AtDateTime, 
                query.Pagination, 
                cancellationToken);

            return articles.Select(x => x.ToDto());
        }
    }
}
