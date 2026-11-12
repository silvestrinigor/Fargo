namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// The event types published when an article changes. Each value is also the
/// RabbitMQ routing key of the event.
/// </summary>
public static class ArticleEventTypes
{
    /// <summary>An article was created.</summary>
    public const string Created = "article.created";

    /// <summary>
    /// An article, or one of its parts (barcode, dimension, pack, kit
    /// components, container, allowed articles, or partitions), was edited.
    /// </summary>
    public const string Edited = "article.edited";

    /// <summary>An article was deleted.</summary>
    public const string Deleted = "article.deleted";
}

/// <summary>
/// The message published when an article is created, edited, or deleted.
/// </summary>
/// <remarks>
/// The message identifies the article only. Consumers that need the article's
/// current state read it through the API; a deleted article has no state left
/// to read. Delivery is at-least-once, so consumers must ignore an
/// <see cref="EventId"/> they have already processed.
/// </remarks>
/// <param name="EventId">The unique identifier of this event, used to discard duplicates.</param>
/// <param name="EventType">One of the <see cref="ArticleEventTypes"/> values.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
/// <param name="ArticleGuid">The unique identifier of the article that changed.</param>
public sealed record ArticleEventMessage(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid ArticleGuid)
{
    /// <summary>
    /// The version of the message format. Increase it on any breaking change.
    /// </summary>
    public const int SchemaVersion = 1;
}
