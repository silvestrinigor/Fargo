using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Articles;

#region Single

public sealed record ArticleSingleQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

public sealed class ArticleSingleQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser
) : IQueryHandler<ArticleSingleQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: null,
            cancellationToken
        );

        return article;
    }
}

#endregion Single

#region Many

public sealed record ArticlesQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? NotInsideAnyPartition = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;

public sealed class ArticlesQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser
) : IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>
{
    public async Task<IReadOnlyCollection<ArticleDto>> Handle(
        ArticlesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : actor.PartitionAccessesGuids;

        var articles = await articleRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        return articles;
    }
}

#endregion Many