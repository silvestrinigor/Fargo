using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries.ArticleQueries
{
    public sealed record ArticleSingleQuery(Guid ArticleGuid) : IQuery<ArticleDto>;

    public sealed class ArticleSingleQueryHandler(IArticleRepository repository) : IQueryHandlerAsync<ArticleSingleQuery, ArticleDto>
    {
        public async Task<ArticleDto> HandleAsync(ArticleSingleQuery query, CancellationToken cancellationToken = default)
        {
            var article = await repository.GetByGuidAsync(query.ArticleGuid)
                ?? throw new InvalidOperationException("Article not found.");

            return article.ToDto();
        }
    }
}
