using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemContainerInventoryByGuidQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item container inventory query by guid started for contianer '{containerGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void QueryInventoryByGuidStarted(
        this ILogger logger, Guid containerGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item container inventory query by guid completed for container '{containerGuid}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}.")]
    public static partial void QueryInventoryByGuidCompleted(
        this ILogger logger, Guid containerGuid, Guid actorGuid, ActorType actorType, bool found);
}
