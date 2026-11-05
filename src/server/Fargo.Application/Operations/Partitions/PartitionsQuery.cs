using Fargo.Application.Common;
using Fargo.Application.Operations.Partitions.Dtos;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Partitions;

public sealed record PartitionsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<PartitionDto>>;

public sealed class PartitionsQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor
) : IQueryHandler<PartitionsQuery, PagedResult<PartitionDto>>
{
    public async Task<PagedResult<PartitionDto>> HandleAsync(
        PartitionsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, partitionGuids,
            query.Sort,
            query.Search,
            cancellationToken);

        return partitions;
    }
}
