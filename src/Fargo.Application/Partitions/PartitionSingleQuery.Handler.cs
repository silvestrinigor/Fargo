using Fargo.Application.Identity;
using Fargo.Application.Shared.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionSingleQueryHandler(
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> HandleAsync(
        PartitionSingleQuery query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query started for partition {partitionGuid} by actor {actorId}.",
                query.PartitionGuid, currentActor.ActorId);
        }

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid, query.AsOfDateTime, childOfAnyOfThesePartitions: null,
            notChildOfAnyPartition: null, cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query completed for partition {partitionGuid} by actor {actorId}. Found: {Found}.",
                query.PartitionGuid,
                currentActor.ActorId,
                partition is not null);
        }

        return partition;
    }
}
