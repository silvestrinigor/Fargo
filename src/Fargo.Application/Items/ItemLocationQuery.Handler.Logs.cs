using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemLocationQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item location query started for item {itemGuid} by actor {actorGuid}.")]
    public static partial void LocationQueryStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Item location query completed for item {itemGuid} by actor {actorGuid}. Found: {found}.")]
    public static partial void LocationQueryCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, bool found);
}
