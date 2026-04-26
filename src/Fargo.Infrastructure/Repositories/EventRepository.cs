using Fargo.Application.Events;
using Fargo.Domain;
using Fargo.Domain.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class EventRepository(FargoDbContext context) : IEventQueryRepository
{
    public async Task<IReadOnlyCollection<EventInformation>> GetMany(
        Guid? entityGuid,
        EntityType? entityType,
        EventType? eventType,
        Guid? actorGuid,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Pagination? pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Event> query = context.Events.AsNoTracking();

        if (entityGuid.HasValue)
        {
            query = query.Where(e => e.EntityGuid == entityGuid.Value);
        }

        if (entityType.HasValue)
        {
            query = query.Where(e => e.EntityType == entityType.Value);
        }

        if (eventType.HasValue)
        {
            query = query.Where(e => e.EventType == eventType.Value);
        }

        if (actorGuid.HasValue)
        {
            query = query.Where(e => e.ActorGuid == actorGuid.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.OccurredAt <= to.Value);
        }

        query = query.OrderByDescending(e => e.OccurredAt);

        if (pagination.HasValue)
        {
            query = query.Skip(pagination.Value.Skip).Take(pagination.Value.Take);
        }

        return await query
            .Select(e => new EventInformation(
                e.Guid,
                e.EventType,
                e.EntityType,
                e.EntityGuid,
                e.ActorGuid,
                e.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}
