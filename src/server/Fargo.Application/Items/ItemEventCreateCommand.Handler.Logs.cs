using Fargo.Core.Actors;
using Fargo.Core.Items.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemEventCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item event flow started ({eventType}) for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ItemEventCreateStarted(
        this ILogger logger, ItemEventType eventType, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item event '{itemEventGuid}' ({eventType}) recorded for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ItemEventCreateCompleted(
        this ILogger logger, Guid itemEventGuid, ItemEventType eventType, Guid articleGuid, Guid actorGuid, ActorType actorType);
}
