using Fargo.Application.IntegrationEvents;
using Fargo.RealtimeContracts;

namespace Fargo.ServiceRealtime.Notifications;

/// <summary>
/// Maps broker article events to the hub's wire contract.
/// </summary>
public static class ArticleEventNotifications
{
    /// <summary>
    /// Creates the notification for an article event.
    /// </summary>
    /// <param name="message">The article event.</param>
    /// <returns>
    /// The notification, or <see langword="null"/> when the event type is not
    /// one of the known <see cref="ArticleEventTypes"/>.
    /// </returns>
    public static EntityChangedNotification? ToNotification(ArticleEventMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var changeType = message.EventType switch
        {
            ArticleEventTypes.Created => EntityChangedNotification.ChangeTypes.Created,
            ArticleEventTypes.Edited => EntityChangedNotification.ChangeTypes.Edited,
            ArticleEventTypes.Deleted => EntityChangedNotification.ChangeTypes.Deleted,
            _ => null,
        };

        return changeType is null
            ? null
            : new EntityChangedNotification(
                EntityChangedNotification.EntityTypes.Article,
                message.ArticleGuid,
                changeType,
                message.EventId,
                message.OccurredAt);
    }
}
