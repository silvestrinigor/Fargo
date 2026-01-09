using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    internal class ItemReadRepository(FargoContext context) : IItemReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Item>> GetManyAsync(
            Guid? articleGuid = null, 
            DateTime? atDateTime = null, 
            int? skip = null, 
            int? take = null, 
            CancellationToken cancellationToken = default
            )
        {
            var query = atDateTime is not null
                ? context.Items.TemporalAsOf(atDateTime.Value)
                : context.Items.AsQueryable();

            if (skip != null)
            {
                query = query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = query.Take(take.Value);
            }

            return await query
                .AsNoTracking()
                .Where(x => articleGuid == null || x.ArticleGuid == articleGuid.Value)
                .Include(x => x.Article)
                .ToListAsync(cancellationToken);
        }

        public async Task<Item?> GetByGuidAsync(
            Guid itemGuid, 
            DateTime? atDateTime, 
            CancellationToken cancellationToken = default
            )
        {
            var query = atDateTime is not null
                ? context.Items.TemporalAsOf(atDateTime.Value)
                : context.Items;

            return await query
                .AsNoTracking()
                .Include(x => x.Article)
                .Where(x => x.Guid == itemGuid)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}