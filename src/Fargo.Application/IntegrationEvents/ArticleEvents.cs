namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// The event types published when an article changes. Each value is also the
/// RabbitMQ routing key of the event.
/// </summary>
public static class ArticleEventTypes
{
    public const string Created = "article.created";

    /// <summary>
    /// An article, or one of its parts (barcode, dimension, pack, kit components, container, allowed
    /// articles, or partitions), was edited.
    /// </summary>
    public const string Edited = "article.edited";

    public const string Deleted = "article.deleted";
}

/// <remarks>
/// <para>
/// The message identifies the article and its partitions, not its state. Consumers that need the
/// current state read it through the API; a deleted article has none left to read. Delivery is
/// at-least-once, so consumers must ignore an <see cref="EventId"/> they have already processed.
/// </para>
/// <para>
/// <see cref="PartitionGuids"/> lets a consumer decide who may learn about the change without
/// reading the database: an actor may see the event only when it can access at least one of these
/// partitions, the same rule the API applies to article reads. It holds every partition the article
/// belonged to before or after the change, so actors that just lost access to a removed partition are
/// told too (their next read returns not found). A deleted article reports the partitions it had
/// when deleted.
/// </para>
/// </remarks>
/// <param name="EventType">One of the <see cref="ArticleEventTypes"/> values.</param>
/// <param name="OccurredAt">UTC instant at which the change was saved.</param>
/// <param name="PartitionGuids">Partitions before or after the change, in ascending order.</param>
public sealed record ArticleEventMessage(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid ArticleGuid,
    IReadOnlyList<Guid> PartitionGuids)
{
    /// <summary>
    /// The version of the message format; increase it on any breaking change.
    /// </summary>
    /// <remarks>
    /// Version 2 added <see cref="PartitionGuids"/>. A consumer that routes by partition must not guess
    /// for a version 1 message, which has none.
    /// </remarks>
    public const int SchemaVersion = 2;
}
