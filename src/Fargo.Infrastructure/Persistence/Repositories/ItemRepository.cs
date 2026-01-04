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

        public async Task<IEnumerable<Item>> GetManyAsync(Guid? articleGuid = null, CancellationToken cancellationToken = default)
        {
            var query = context.Items.AsQueryable();

            if (articleGuid is not null)
            {
                query = query.Where(x => x.ArticleGuid == articleGuid.Value);
            }

            return await query
                .Include(x => x.Article)
                .ToListAsync(cancellationToken);
        }

        public async Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default)
        {
            return await context.Items
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x => x.Guid == itemGuid, cancellationToken);
        }

        public void Remove(Item item)
        {
            context.Items.Remove(item);
        }
    }
}
