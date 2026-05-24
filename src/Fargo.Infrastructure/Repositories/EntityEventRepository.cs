using Fargo.Core.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class EntityEventRepository(FargoDbContext context) : IEntityEventRepository
{
    private readonly DbSet<EntityEvent> entityEvents = context.EntityEvents;

    public void Add(EntityEvent entityEvent) => entityEvents.Add(entityEvent);
}

public sealed class EntityPartitionEventRepository(FargoDbContext context) : IEntityPartitionEventRepository
{
    private readonly DbSet<EntityPartitionEvent> entityPartitionEvents = context.EntityPartitionEvents;

    public void Add(EntityPartitionEvent entityPartitionEvent) => entityPartitionEvents.Add(entityPartitionEvent);
}
