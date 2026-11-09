using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;


namespace Fargo.Application.Operations.Items;

public sealed record ItemSingleQuery(Guid ItemGuid) : IQuery<ItemDto?>;

public sealed class ItemSingleQueryHandler(
    ActorResolver actorService, IItemQueryRepository itemRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> HandleAsync(
        ItemSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var item = await itemRepository.GetInfoByGuidAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken
        );

        return item;
    }
}
