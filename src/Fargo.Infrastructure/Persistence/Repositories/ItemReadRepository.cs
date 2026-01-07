using Fargo.Domain.Entities.Models;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    internal class ItemReadRepository(FargoContext context) : IItemReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Item>> GetManyAsync(Guid? articleGuid = null, CancellationToken cancellationToken = default)
        {
            var query = context.Items
                .AsQueryable()
                .AsNoTracking();

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
                .AsNoTracking()
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x => x.Guid == itemGuid, cancellationToken);
        }
    }
}