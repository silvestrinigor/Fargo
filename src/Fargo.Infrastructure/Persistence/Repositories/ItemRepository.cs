using Fargo.Domain.Entities.ArticleItems;
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

        public async Task<Item?> GetByGuidAsync(
            Guid itemGuid, 
            CancellationToken cancellationToken = default
            )
            => await context.Items
                .Include(x => x.Article)
                .Where(x => x.Guid == itemGuid)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<bool> IsInsideContainer(Item item, Item container, CancellationToken cancellationToken = default)
        {
            if (!container.Article.IsContainer)
            {
                return false;
            }

            Guid? currentParentId = item.ParentItemGuid;

            while (currentParentId is not null)
            {
                if (currentParentId == container.Guid)
                    return true;

                currentParentId = await context.Items
                    .Where(x => x.Guid == currentParentId)
                    .Select(x => x.ParentItemGuid)
                    .SingleOrDefaultAsync(cancellationToken);
            }

            return false;
        }
    }
}
