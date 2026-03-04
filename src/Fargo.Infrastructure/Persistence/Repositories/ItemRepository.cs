using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ItemRepository(FargoWriteDbContext context) : IItemRepository
    {
        private readonly DbSet<Item> items = context.Items;

        public void Add(Item item)
            => context.Items.Add(item);

        public void Remove(Item item)
            => context.Items.Remove(item);

        public async Task<Item?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                )
            => await items
            .Where(a =>
                    a.Guid == entityGuid
                  )
            .SingleOrDefaultAsync(cancellationToken);
    }
}