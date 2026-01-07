using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ArticleSingleQuery(Guid ArticleGuid) : IQuery<ArticleDto>;

    public sealed class ArticleSingleQueryHandler(IModelReadRepository repository) : IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?>
    {
        private readonly IModelReadRepository repository = repository;

        public async Task<ArticleDto?> HandleAsync(ArticleSingleQuery query, CancellationToken cancellationToken = default)
        {
            var article = await repository.GetByGuidAsync(query.ArticleGuid, cancellationToken) as Article;

            return article?.ToDto();
        }
    }
}
