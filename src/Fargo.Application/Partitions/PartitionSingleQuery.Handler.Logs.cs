using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partition single query started for partition {partitionGuid} by actor {actorId}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partition single query completed for partition {partitionGuid} by actor {actorId}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId,
        bool found);
}
