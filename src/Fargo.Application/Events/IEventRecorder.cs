using Fargo.Domain.Events;

namespace Fargo.Application.Events;

/// <summary>
/// Records domain events in persistent storage as part of the current unit of work.
/// </summary>
public interface IEventRecorder
{
    Task Record(EventType eventType, EntityType entityType, Guid entityGuid, CancellationToken cancellationToken = default);
}
