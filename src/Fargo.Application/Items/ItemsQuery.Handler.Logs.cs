using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemsQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Items query started for actor {actorGuid}. Page: {page}. Limit: {limit}.")]
    public static partial void ManyQueryStarted(
        this ILogger logger,
        Guid actorGuid, Page page, Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Items query completed for actor {actorGuid}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ManyQueryCompleted(
        this ILogger logger,
        Guid actorGuid,
        int requestedPartitionCount,
        int effectivePartitionCount,
        int resultCount);
}
