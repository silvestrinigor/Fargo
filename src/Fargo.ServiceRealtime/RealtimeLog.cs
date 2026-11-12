using Fargo.Core.Entities.Actors;

namespace Fargo.ServiceRealtime;

/// <summary>
/// Source-generated log messages of the real-time host.
/// </summary>
internal static partial class RealtimeLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Connection {ConnectionId} opened for {ActorType} {ActorGuid}.")]
    internal static partial void ConnectionOpened(this ILogger logger, string connectionId, Guid actorGuid, ActorType actorType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connection {ConnectionId} rejected: actor {ActorGuid} was not found or is inactive.")]
    internal static partial void ConnectionRejected(this ILogger logger, string connectionId, Guid actorGuid);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Event {EventId} ({EventType}) sent to {ConnectionCount} connections.")]
    internal static partial void NotificationSent(this ILogger logger, Guid eventId, string eventType, int connectionCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Event {EventId} has the unknown type {EventType}; it is not pushed.")]
    internal static partial void UnknownEventType(this ILogger logger, Guid eventId, string eventType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Refreshing the partition access of open connections failed; keeping the last known access.")]
    internal static partial void AccessRefreshFailed(this ILogger logger, Exception exception);
}
