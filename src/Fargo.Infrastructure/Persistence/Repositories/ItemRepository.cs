using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ItemRepository(FargoContext context) : IItemRepository
    {
        private readonly FargoContext context = context;

        public void Add(Item item)
        {
            context.Items.Add(item);
        }

        public async Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default)
        {
            return await context.Items.FirstOrDefaultAsync(x => x.Guid == itemGuid, cancellationToken);
        }

        public void Remove(Item item)
        {
            context.Items.Remove(item);
        }
    }
}
