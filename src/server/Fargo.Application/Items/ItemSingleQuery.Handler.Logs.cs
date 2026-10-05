using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item single query started for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void SingleQueryStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item single query completed for item {itemGuid} by actor {actorGuid} of type '{actorType}'. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType, bool found
    );
}
