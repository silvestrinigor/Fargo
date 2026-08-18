using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemLocationQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item location query started for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void LocationQueryStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item location query completed for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}.")]
    public static partial void LocationQueryCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType, bool found
    );
}
