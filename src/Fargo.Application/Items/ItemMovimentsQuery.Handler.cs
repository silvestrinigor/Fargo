using Fargo.Application.Common;
using Fargo.Application.Identity;
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
        logger.MovimentsQueryStarted(query.ItemGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var itemMoviments = await itemQueryRepository.GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.MovimentsQueryCompleted(query.ItemGuid, currentActor.Guid, itemMoviments is not null);

        return itemMoviments;
    }
}
