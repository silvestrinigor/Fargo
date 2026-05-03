using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Articles;

public sealed record ArticleManyQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? InsideNoPartition = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;

public sealed class ArticleManyQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser
) : IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleDto>>
{
    public async Task<IReadOnlyCollection<ArticleDto>> Handle(
        ArticleManyQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        return await articleRepository.GetManyInfoInPartitionsOrPublic(
            query.WithPagination,
            actor.PartitionAccesses,
            query.TemporalAsOfDateTime,
            cancellationToken
        );
    }
}
