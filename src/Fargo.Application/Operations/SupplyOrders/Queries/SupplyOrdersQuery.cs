using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// The text search is case-insensitive over the reference and description.
/// </summary>
public sealed record SupplyOrdersQuery(
    Pagination WithPagination,
    SupplyOrderStatus? Status = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<SupplyOrderDto>>;

public sealed class SupplyOrdersQueryHandler(
    ActorResolver actorService,
    ISupplyOrderQueryRepository supplyOrderRepository,
    ICurrentActor currentActor
) : IQueryHandler<SupplyOrdersQuery, PagedResult<SupplyOrderDto>>
{
    public async Task<PagedResult<SupplyOrderDto>> HandleAsync(
        SupplyOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReadSupplyOrders);

        var partitionGuids = PartitionQueryFilter.ForPartitionedEntities(
            actor.PartitionAccessGuids,
            query.ChildOfAnyOfThesePartitions);

        return await supplyOrderRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            query.Status,
            query.Sort,
            query.Search,
            cancellationToken);
    }
}
