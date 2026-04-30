using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Domain.Events;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Events;

/// <summary>
/// Stores domain events in the current EF Core unit of work.
/// </summary>
public sealed class DbEventRecorder(
    FargoDbContext db,
    ICurrentUser currentUser
) : IEventRecorder
{
    public Task Record(EventType eventType, EntityType entityType, Guid entityGuid, CancellationToken cancellationToken = default)
    {
        db.Events.Add(new Event
        {
            EventType = eventType,
            EntityType = entityType,
            EntityGuid = entityGuid,
            ActorGuid = currentUser.UserGuid,
        });

        return Task.CompletedTask;
    }
}
