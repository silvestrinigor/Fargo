using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemEventsQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item events query started by actor '{actorGuid}' of type '{actorType}' for page '{page}' with limit '{limit}'.")]
    public static partial void ItemEventsQueryStarted(
        this ILogger logger, Guid actorGuid, ActorType actorType, Page page, Limit limit
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item events query completed by actor '{actorGuid}' of type '{actorType}'. Accessible partitions: {partitionCount}. Returned: {returned}.")]
    public static partial void ItemEventsQueryCompleted(
        this ILogger logger, Guid actorGuid, ActorType actorType, int partitionCount, int returned
    );
}
