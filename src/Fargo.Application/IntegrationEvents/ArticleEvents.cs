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
/// <para>
/// The message identifies the article and its partitions, not its state.
/// Consumers that need the article's current state read it through the API; a
/// deleted article has no state left to read. Delivery is at-least-once, so
/// consumers must ignore an <see cref="EventId"/> they have already processed.
/// </para>
/// <para>
/// <see cref="PartitionGuids"/> lets a consumer decide who may learn about the
/// change without reading the database: an actor may see the event only when it
/// can access at least one of these partitions, the same rule the API applies to
/// article reads. It holds every partition the article belonged to before or
/// after the change, so when a partition is removed the actors that just lost
/// access are told too (their next read returns not found). A deleted article
/// reports the partitions it had when it was deleted.
/// </para>
/// </remarks>
/// <param name="EventId">The unique identifier of this event, used to discard duplicates.</param>
/// <param name="EventType">One of the <see cref="ArticleEventTypes"/> values.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
/// <param name="ArticleGuid">The unique identifier of the article that changed.</param>
/// <param name="PartitionGuids">
/// The partitions the article belonged to before or after the change, in
/// ascending order.
/// </param>
public sealed record ArticleEventMessage(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid ArticleGuid,
    IReadOnlyList<Guid> PartitionGuids)
{
    /// <summary>
    /// The version of the message format. Increase it on any breaking change.
    /// </summary>
    /// <remarks>
    /// Version 2 added <see cref="PartitionGuids"/>. A consumer that routes by
    /// partition must not guess for a version 1 message, which has none.
    /// </remarks>
    public const int SchemaVersion = 2;
}
