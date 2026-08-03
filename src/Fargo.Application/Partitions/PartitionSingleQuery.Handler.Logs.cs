using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partition single query started for partition {partitionGuid} by actor {actorGuid}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger,
        Guid partitionGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partition single query completed for partition {partitionGuid} by actor {actorGuid}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger,
        Guid partitionGuid, Guid actorGuid, bool found);
}
