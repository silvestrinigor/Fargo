namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// An integration event waiting to be published, stored in the same database
/// transaction as the change that produced it (transactional outbox).
/// </summary>
/// <remarks>
/// Rows are written by the outbox save-changes interceptor (Fargo.Infrastructure) and read only
/// by the outbox relay (Fargo.ServiceOutbox), which publishes them in <see cref="Sequence"/>
/// order and marks them processed. Processed rows are purged after a retention
/// period.
/// </remarks>
public sealed class OutboxMessage
{
    /// <summary>
    /// The maximum length stored in <see cref="LastError"/>.
    /// </summary>
    public const int LastErrorMaxLength = 2000;

    /// <summary>
    /// The maximum length of <see cref="EventType"/>.
    /// </summary>
    public const int EventTypeMaxLength = 200;

    /// <summary>
    /// Gets the database-generated, increasing position of the message, which
    /// defines the publishing order.
    /// </summary>
    public long Sequence { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the event. Consumers use it to discard
    /// duplicates.
    /// </summary>
    public Guid EventId { get; private init; }

    /// <summary>
    /// Gets the event type, which is also the routing key.
    /// </summary>
    public string EventType { get; private init; } = null!;

    /// <summary>
    /// Gets the version of the message format in <see cref="Body"/>.
    /// </summary>
    public int SchemaVersion { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the entity the event is about.
    /// </summary>
    public Guid AggregateGuid { get; private init; }

    /// <summary>
    /// Gets the instant, in UTC, at which the change was saved.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private init; }

    /// <summary>
    /// Gets the serialized JSON message, published exactly as stored.
    /// </summary>
    public string Body { get; private init; } = null!;

    /// <summary>
    /// Gets the instant, in UTC, at which the broker accepted the message, or
    /// <see langword="null"/> while it is still waiting to be published.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>
    /// Gets the number of failed publishing attempts.
    /// </summary>
    public int Attempts { get; private set; }

    /// <summary>
    /// Gets the instant, in UTC, of the last failed publishing attempt.
    /// </summary>
    public DateTimeOffset? LastAttemptAt { get; private set; }

    /// <summary>
    /// Gets the error of the last failed publishing attempt.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Initializes a new instance. Required by Entity Framework.
    /// </summary>
    private OutboxMessage() { }

    /// <summary>
    /// Creates the outbox message for an article event.
    /// </summary>
    /// <param name="message">The article event.</param>
    /// <returns>A new, unprocessed outbox message.</returns>
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

    /// <summary>
    /// Converts the message to the form handed to an <see cref="IEventPublisher"/>.
    /// </summary>
    public OutboundEvent ToOutboundEvent()
        => new(EventId, EventType, SchemaVersion, OccurredAt, Body);

    /// <summary>
    /// Records that the broker accepted the message.
    /// </summary>
    /// <param name="processedAt">The instant the broker accepted the message.</param>
    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
    }

    /// <summary>
    /// Records a failed publishing attempt.
    /// </summary>
    /// <param name="attemptedAt">The instant of the attempt.</param>
    /// <param name="error">The error description.</param>
    public void RecordFailure(DateTimeOffset attemptedAt, string error)
    {
        Attempts++;
        LastAttemptAt = attemptedAt;
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;
    }
}
