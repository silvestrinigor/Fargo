using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.ArticleQueries;

public sealed record ArticleManyQuery(
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<ArticleInformation>>;

public sealed class ArticleManyQueryHandler(
        IArticleRepository articleRepository
        )
    : IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleInformation>>
{
    public async Task<IReadOnlyCollection<ArticleInformation>> Handle(
            ArticleManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        var articles = await articleRepository.GetManyInfo(
                query.Pagination ?? Pagination.First20Pages,
                query.AsOfDateTime,
                cancellationToken
                );

        return articles;
    }
}
