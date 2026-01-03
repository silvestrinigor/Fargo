using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ArticleAllQuery() : IQuery<IEnumerable<ArticleDto>>;

    public sealed class ArticleAllQueryHandler(IArticleRepository repository) : IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>>
    {
        public async Task<IEnumerable<ArticleDto>> HandleAsync(ArticleAllQuery query, CancellationToken cancellationToken = default)
        {
            var articles = await repository.GetAllAsync(cancellationToken);

            return articles.Select(x => x.ToDto());
        }
    }
}
