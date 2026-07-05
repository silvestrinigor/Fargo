using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition delete flow started for partition {partitionGuid} by actor {actorId}.")]
    public static partial void DeleteStarted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition delete mutation completed for partition {partitionGuid} by actor {actorId}.")]
    public static partial void DeleteCompleted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);
}
