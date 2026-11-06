namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// An event ready to leave Fargo, as stored in the outbox.
/// </summary>
/// <param name="EventId">The unique identifier of the event; brokers use it as the message id.</param>
/// <param name="EventType">The event type; brokers use it as the routing key.</param>
/// <param name="SchemaVersion">The version of the message format in <paramref name="Body"/>.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
/// <param name="Body">The serialized JSON message.</param>
public sealed record OutboundEvent(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    string Body);

/// <summary>
/// Delivers outbound events to a message broker.
/// </summary>
/// <remarks>
/// Implementations must only return once the broker has accepted the event (for
/// example after a publisher confirm) and must throw otherwise, so the outbox
/// relay keeps the event and retries it. The same event may be delivered more
/// than once.
/// </remarks>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event and waits until the broker has accepted it.
    /// </summary>
    /// <param name="outboundEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task PublishAsync(OutboundEvent outboundEvent, CancellationToken cancellationToken = default);
}
