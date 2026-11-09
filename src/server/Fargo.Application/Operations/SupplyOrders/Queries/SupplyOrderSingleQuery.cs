using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;


namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// Retrieves a single supply order by its identifier.
/// </summary>
public sealed record SupplyOrderSingleQuery(Guid SupplyOrderGuid) : IQuery<SupplyOrderDto?>;

public sealed class SupplyOrderSingleQueryHandler(
    ActorResolver actorService,
    ISupplyOrderQueryRepository supplyOrderRepository,
    ICurrentActor currentActor
) : IQueryHandler<SupplyOrderSingleQuery, SupplyOrderDto?>
{
    public async Task<SupplyOrderDto?> HandleAsync(
        SupplyOrderSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReadSupplyOrders);

        return await supplyOrderRepository.GetInfoByGuidAsync(
            query.SupplyOrderGuid,
            actor.PartitionAccessGuids,
            cancellationToken);
    }
}
