using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionsQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partitions query started for actor {actorId}. Page: {page}. Limit: {limit}.")]
    public static partial void ManyQueryStarted(
        this ILogger logger,
        ActorId actorId, Page page, Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Partitions query completed for actor {actorId}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ManyQueryCompleted(
        this ILogger logger,
        ActorId actorId, int requestedPartitionCount, int effectivePartitionCount, int resultCount);
}
