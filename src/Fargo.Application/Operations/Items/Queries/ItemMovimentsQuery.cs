using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;


namespace Fargo.Application.Operations.Items;

public sealed record ItemMovimentsQuery(
    Guid ItemGuid
) : IQuery<IReadOnlyCollection<ItemMovimentDto>?>;

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
