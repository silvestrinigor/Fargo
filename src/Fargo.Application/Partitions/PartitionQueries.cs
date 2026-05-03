using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Partitions;

#region Single

public sealed record PartitionSingleQuery(
    Guid PartitionGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<PartitionDto?>;

public sealed class PartitionSingleQueryHandler(
    ActorService actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> Handle(
        PartitionSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (!actor.PartitionAccessesGuids.Contains(query.PartitionGuid))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: null,
            cancellationToken
        );

        return partition;
    }
}

#endregion Single

#region Many

public sealed record PartitionsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? NotInsideAnyPartition = null
) : IQuery<IReadOnlyCollection<PartitionDto>>;

public sealed class PartitionsQueryHandler(
    ActorService actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> Handle(
        PartitionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : actor.PartitionAccessesGuids;

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        if (partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p.Guid)))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

        return partitions;
    }
}

#endregion Many
