using Fargo.Application.Dtos.ArticlesDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ArticleSingleQuery(
        Guid ArticleGuid,
        DateTime? AtDateTime
        ) : IQuery<ArticleDto>;

    public sealed class ArticleSingleQueryHandler(IArticleReadRepository repository) : IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?>
    {
        private readonly IArticleReadRepository repository = repository;

        public async Task<ArticleDto?> HandleAsync(ArticleSingleQuery query, CancellationToken cancellationToken = default)
        {
            var article = await repository.GetByGuidAsync(query.ArticleGuid, query.AtDateTime, cancellationToken);

            return article?.ToDto();
        }
    }
}
