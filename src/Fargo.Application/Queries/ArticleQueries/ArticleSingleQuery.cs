using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.ArticleQueries;

public sealed record ArticleSingleQuery(
        Guid ArticleGuid,
        DateTimeOffset? AsOfDateTime = null
        ) : IQuery<ArticleInformation?>;

public sealed class ArticleSingleQueryHandler(
        IArticleRepository articleRepository
        ) : IQueryHandler<ArticleSingleQuery, ArticleInformation?>
{
    public async Task<ArticleInformation?> Handle(
            ArticleSingleQuery query,
            CancellationToken cancellationToken = default)
    {
        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            cancellationToken
            );

        return article;
    }
}
