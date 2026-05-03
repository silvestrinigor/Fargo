using Fargo.Domain.Events;

namespace Fargo.Application.Events;

/// <summary>Persistence contract for read-side event queries.</summary>
public interface IEventQueryRepository
{
    /// <summary>Returns a paged, optionally filtered list of events ordered by <c>OccurredAt</c> descending.</summary>
    Task<IReadOnlyCollection<EventInformation>> GetMany(
        Guid? entityGuid,
        EntityType? entityType,
        EventType? eventType,
        Guid? actorGuid,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Pagination? pagination,
        CancellationToken cancellationToken = default
    );
}
