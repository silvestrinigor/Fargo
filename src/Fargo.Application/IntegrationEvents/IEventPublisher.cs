namespace Fargo.Application.IntegrationEvents;

/// <summary>
/// An event ready to leave Fargo, as stored in the outbox.
/// </summary>
/// <param name="EventId">Brokers use it as the message id.</param>
/// <param name="EventType">Brokers use it as the routing key.</param>
/// <param name="OccurredAt">The instant, in UTC, at which the change was saved.</param>
public sealed record OutboundEvent(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    string Body);

/// <remarks>
/// Implementations must only return once the broker has accepted the event (for example after a
/// publisher confirm) and must throw otherwise, so the outbox relay keeps the event and retries it.
/// The same event may be delivered more than once.
/// </remarks>
public interface IEventPublisher
{
    Task PublishAsync(OutboundEvent outboundEvent, CancellationToken cancellationToken = default);
}
