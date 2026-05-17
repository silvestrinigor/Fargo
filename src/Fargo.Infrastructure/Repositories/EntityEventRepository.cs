using Fargo.Core;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class EntityEventRepository(FargoDbContext context) : IEntityEventRepository
{
    private readonly DbSet<EntityEvent> entityEvents = context.EntityEvents;

    public void Add(EntityEvent entityEvent) => entityEvents.Add(entityEvent);
}
