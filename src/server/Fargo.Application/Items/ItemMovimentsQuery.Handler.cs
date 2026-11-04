using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Items;

public sealed class ItemMovimentsQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemMovimentsQuery, IReadOnlyCollection<ItemMovimentDto>?>
{
    public async Task<IReadOnlyCollection<ItemMovimentDto>?> HandleAsync(
        ItemMovimentsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var itemMoviments = await itemQueryRepository.GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken
        );

        return itemMoviments;
    }
}
