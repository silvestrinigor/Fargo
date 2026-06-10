using Fargo.Core.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class EntityEventRepository(FargoDbContext context) : IEventRepository
{
    private readonly DbSet<Event> entityEvents = context.EntityEvents;

    public void Add(Event entityEvent) => entityEvents.Add(entityEvent);
}

public sealed class EntityPartitionEventRepository(FargoDbContext context) : IPartitionEventRepository
{
    private readonly DbSet<PartitionEvent> entityPartitionEvents = context.EntityPartitionEvents;

    public void Add(PartitionEvent entityPartitionEvent) => entityPartitionEvents.Add(entityPartitionEvent);
}
