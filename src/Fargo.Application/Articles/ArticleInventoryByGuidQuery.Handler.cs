using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleInventoryByGuidQueryHandler(
    ActorResolver actorService, IArticleQueryRepository articleRepository, IItemRepository itemRepository,
    ICurrentActor currentActor, ILogger<ArticleInventoryByGuidQueryHandler> logger
) : IQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?>
{
    public async Task<ArticleInventoryDto?> HandleAsync(
        ArticleInventoryByGuidQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryInventoryByGuidStarted(query.ArticleGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        ArticleInventoryDto? inventory;

        if (query.IncludeDescendents && query.InsideItemContainerGuids is { Count: > 0 })
        {
            var itemContainerGuids = new List<Guid>();

            foreach (var containerGuid in query.InsideItemContainerGuids)
            {
                var result = await itemRepository.GetContainedDescendantGuidsAsync(containerGuid, true, cancellationToken);

                itemContainerGuids.AddRange(result);
            }

            inventory = await articleRepository.GetInventoryInfoByGuidAsync(
                query.ArticleGuid, [.. itemContainerGuids.Distinct()], actor.PartitionAccessGuids, cancellationToken);
        }
        else
        {
            inventory = await articleRepository.GetInventoryInfoByGuidAsync(
                query.ArticleGuid, query.InsideItemContainerGuids, actor.PartitionAccessGuids, cancellationToken);
        }

        logger.QueryInventoryByGuidCompleted(
            query.ArticleGuid, currentActor.Guid, currentActor.ActorType, inventory is not null, inventory?.TotalCount ?? 0);

        return inventory;
    }
}
