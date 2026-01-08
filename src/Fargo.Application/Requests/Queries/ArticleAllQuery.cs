using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ArticleAllQuery(
        DateTime? AtDateTime,
        PaginationDto Pagination
        ) : IQuery<IEnumerable<ArticleDto>>;

    public sealed class ArticleAllQueryHandler(IArticleReadRepository repository) : IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<IEnumerable<ArticleDto>> HandleAsync(ArticleAllQuery query, CancellationToken cancellationToken = default)
        {
            var articles = await repository.GetAllAsync(query.AtDateTime, query.Pagination.Skip, query.Pagination.Limit, cancellationToken);

            return articles.Select(x => x.ToDto());
        }
    }
}
