using Fargo.Application.IntegrationEvents;

namespace Fargo.ServiceRealtime.Hubs;

/// <summary>
/// Tells a client that an entity changed. It carries no entity state: the
/// client reads the entity again through the API, which applies the actor's
/// permissions and partition access to that read.
/// </summary>
/// <param name="EntityType">The kind of entity, for example <c>article</c>.</param>
/// <param name="Guid">The unique identifier of the entity.</param>
/// <param name="ChangeType">One of <see cref="ChangeTypes"/>.</param>
/// <param name="EventId">The event identifier; delivery is at-least-once, so a client may see it twice.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
public sealed record EntityChangedNotification(
    string EntityType,
    Guid Guid,
    string ChangeType,
    Guid EventId,
    DateTimeOffset OccurredAt)
{
    /// <summary>
    /// The <see cref="EntityType"/> of article notifications.
    /// </summary>
    public const string ArticleEntityType = "article";

    /// <summary>
    /// The values of <see cref="ChangeType"/>.
    /// </summary>
    public static class ChangeTypes
    {
        /// <summary>The entity was created.</summary>
        public const string Created = "created";

        /// <summary>The entity, or one of its parts, was edited.</summary>
        public const string Edited = "edited";

        /// <summary>The entity was deleted.</summary>
        public const string Deleted = "deleted";
    }

    /// <summary>
    /// Creates the notification for an article event.
    /// </summary>
    /// <param name="message">The article event.</param>
    /// <returns>
    /// The notification, or <see langword="null"/> when the event type is not
    /// one of the known <see cref="ArticleEventTypes"/>.
    /// </returns>
    public static EntityChangedNotification? FromArticleEvent(ArticleEventMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var changeType = message.EventType switch
        {
            ArticleEventTypes.Created => ChangeTypes.Created,
            ArticleEventTypes.Edited => ChangeTypes.Edited,
            ArticleEventTypes.Deleted => ChangeTypes.Deleted,
            _ => null,
        };

        return changeType is null
            ? null
            : new EntityChangedNotification(
                ArticleEntityType,
                message.ArticleGuid,
                changeType,
                message.EventId,
                message.OccurredAt);
    }
}
