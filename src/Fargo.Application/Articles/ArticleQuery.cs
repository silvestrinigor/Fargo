using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Articles;

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

        if (article is not null && article.Partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p)))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

        return article;
    }
}
