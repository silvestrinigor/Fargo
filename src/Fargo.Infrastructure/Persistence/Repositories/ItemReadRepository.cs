using Fargo.Domain.Entities.Models;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    internal class ItemReadRepository(FargoContext context) : IItemReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Item>> GetManyAsync(Guid? articleGuid = null, CancellationToken cancellationToken = default)
            => await context.Items
                .AsNoTracking()
                .Where(x => articleGuid == null || x.ArticleGuid == articleGuid.Value)
                .Include(x => x.Article)
                .ToListAsync(cancellationToken);

        public async Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default)
            => await context.Items
                .AsNoTracking()
                .Include(x => x.Article)
                .Where(x => x.Guid == itemGuid)
                .FirstOrDefaultAsync(cancellationToken);
    }
}