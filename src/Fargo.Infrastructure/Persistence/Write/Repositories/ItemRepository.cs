using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public class ItemRepository(FargoWriteDbContext context) : EntityByGuidRepository<Item>(context.Items), IItemRepository
    {
        private readonly FargoWriteDbContext context = context;

        public void Add(Item item)
            => context.Items.Add(item);

        public void Remove(Item item)
            => context.Items.Remove(item);

        public async Task<bool> IsInsideContainer(Item item, Item otherItem, CancellationToken cancellationToken = default)
        {
            if (!otherItem.Article.IsContainer)
                return false;

            var currentParentGuid = item.ParentItemGuid;

            while (currentParentGuid is not null)
            {
                if (currentParentGuid == otherItem.Guid)
                    return true;

                currentParentGuid = await context.Items
                    .Where(x => x.Guid == currentParentGuid)
                    .Select(x => x.ParentItemGuid)
                    .SingleOrDefaultAsync(cancellationToken);
            }

            return false;
        }
    }
}
