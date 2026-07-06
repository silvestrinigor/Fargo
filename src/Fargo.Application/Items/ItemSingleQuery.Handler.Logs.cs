using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item single query started for item {itemGuid} by actor {actorId}.")]
    public static partial void SingleQueryStarted(
        this ILogger logger, Guid itemGuid, ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item single query completed for item {itemGuid} by actor {actorID}. Found: {found}.")]
    public static partial void SingleQueryCompleted(
        this ILogger logger, Guid itemGuid, ActorId actorId, bool found);
}
