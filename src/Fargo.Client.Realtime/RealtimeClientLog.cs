using Microsoft.Extensions.Logging;

namespace Fargo.Client.Realtime;

internal static partial class RealtimeClientLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to {HubUri}.")]
    internal static partial void Connected(this ILogger logger, Uri hubUri);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reconnected to {HubUri}.")]
    internal static partial void Reconnected(this ILogger logger, Uri hubUri);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Connection to {HubUri} closed; reconnecting.")]
    internal static partial void ConnectionLost(this ILogger logger, Exception? exception, Uri hubUri);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Reconnect attempt {Attempt} failed.")]
    internal static partial void ReconnectFailed(this ILogger logger, Exception exception, long attempt);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dropped duplicate event {EventId}.")]
    internal static partial void DuplicateDropped(this ILogger logger, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "A {EventName} handler threw; the connection is unaffected.")]
    internal static partial void HandlerFailed(this ILogger logger, Exception exception, string eventName);
}
