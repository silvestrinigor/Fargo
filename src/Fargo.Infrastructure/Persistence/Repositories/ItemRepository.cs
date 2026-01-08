using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ItemRepository(FargoContext context) : IItemRepository
    {
        private readonly FargoContext context = context;

        public void Add(Item item)
            => context.Items.Add(item);

        public void Remove(Item item)
            => context.Items.Remove(item);

        public async Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default)
            => await context.Items
                .Include(x => x.Article)
                .Where(x => x.Guid == itemGuid)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
