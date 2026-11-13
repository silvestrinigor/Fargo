namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// An integration event waiting to be published, stored in the same database
/// transaction as the change that produced it (transactional outbox).
/// </summary>
/// <remarks>
/// Rows are written by the outbox save-changes interceptor (Fargo.Infrastructure) and read only
/// by the outbox relay (Fargo.OutboxRelay), which publishes them in <see cref="Sequence"/>
/// order and marks them processed. Processed rows are purged after a retention
/// period.
/// </remarks>
public sealed class OutboxMessage
{
    public const int LastErrorMaxLength = 2000;

    public const int EventTypeMaxLength = 200;

    /// <summary>
    /// Database-generated, increasing position that defines the publishing order.
    /// </summary>
    public long Sequence { get; private set; }

    public Guid EventId { get; private init; }

    /// <summary>
    /// Also the routing key.
    /// </summary>
    public string EventType { get; private init; } = null!;

    public int SchemaVersion { get; private init; }

    public Guid AggregateGuid { get; private init; }

    public DateTimeOffset OccurredAt { get; private init; }

    /// <summary>
    /// The serialized JSON message, published exactly as stored.
    /// </summary>
    public string Body { get; private init; } = null!;

    /// <summary>
    /// Null while the message is still waiting to be published.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset? LastAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    // Required by EF Core.
    private OutboxMessage() { }

    public static OutboxMessage FromArticleEvent(ArticleEventMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new OutboxMessage
        {
            EventId = message.EventId,
            EventType = message.EventType,
            SchemaVersion = ArticleEventMessage.SchemaVersion,
            AggregateGuid = message.ArticleGuid,
            OccurredAt = message.OccurredAt,
            Body = IntegrationEventJson.Serialize(message),
        };
    }

    public OutboundEvent ToOutboundEvent()
        => new(EventId, EventType, SchemaVersion, OccurredAt, Body);

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
    }

    public void RecordFailure(DateTimeOffset attemptedAt, string error)
    {
        Attempts++;
        LastAttemptAt = attemptedAt;
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;
    }
}
