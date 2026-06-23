using Fargo.Application.Shared.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

#region Single

public sealed record PartitionSingleQuery(
    Guid PartitionGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<PartitionDto?>;

public sealed class PartitionSingleQueryHandler(
    IPartitionQueryRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> HandleAsync(
        PartitionSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query started for partition {PartitionGuid} by actor {ActorGuid}.",
                query.PartitionGuid,
                actor.Guid);
        }

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: null,
            notChildOfAnyPartition: null,
            cancellationToken
        );

        if (partition is not null)
        {
            actor.ValidateHasAccess(partition.Guid);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query completed for partition {PartitionGuid} by actor {ActorGuid}. Found: {Found}.",
                query.PartitionGuid,
                actor.Guid,
                partition is not null);
        }

        return partition;
    }
}

#endregion Single

#region Many

public sealed record PartitionsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<PartitionDto>>;

public sealed class PartitionsQueryHandler(
    IPartitionQueryRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> HandleAsync(
        PartitionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actor.Guid,
                pagination.Page,
                pagination.Limit);
        }

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessesGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var partitions = await partitionRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                partitions.Count);
        }

        return partitions;
    }
}

#endregion Many
