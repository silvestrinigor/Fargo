using Fargo.Application.Authentication;
using Fargo.Domain;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

#region Single

public sealed record PartitionSingleQuery(
    Guid PartitionGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<PartitionDto?>;

public sealed class PartitionSingleQueryHandler(
    ActorService actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentUser currentUser,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> Handle(
        PartitionSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug(
            "Partition single query started for partition {PartitionGuid} by actor {ActorGuid}.",
            query.PartitionGuid,
            currentUser.UserGuid);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: null,
            cancellationToken
        );

        logger.LogDebug(
            "Partition single query completed for partition {PartitionGuid} by actor {ActorGuid}. Found: {Found}.",
            query.PartitionGuid,
            actor.Guid,
            partition is not null);

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
    ICurrentUser currentUser,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> Handle(
        PartitionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug(
            "Partitions query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
            currentUser.UserGuid,
            query.WithPagination.Page,
            query.WithPagination.Limit);

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

        logger.LogDebug(
            "Partitions query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
            actor.Guid,
            query.InsideAnyOfThisPartitions?.Count ?? 0,
            insideAnyOfThisPartitions?.Count ?? 0,
            partitions.Count);

        return partitions;
    }
}

#endregion Many
