using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemMovimentsQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemQueryRepository,
    ICurrentActor currentActor,
    ILogger<ItemMovimentsQueryHandler> logger
) : IQueryHandler<ItemMovimentsQuery, IReadOnlyCollection<ItemMovimentDto>?>
{
    public async Task<IReadOnlyCollection<ItemMovimentDto>?> HandleAsync(
        ItemMovimentsQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Item moviments query", "item", query.ItemGuid);

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var itemMoviments = await itemQueryRepository.GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken
        );

        logger.QueryCompleted("Item moviments query", "item", query.ItemGuid, found: itemMoviments is not null);

        return itemMoviments;
    }
}
