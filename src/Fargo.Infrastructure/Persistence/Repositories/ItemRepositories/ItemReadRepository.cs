using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.ItemRepositories;
using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories.ItemRepositories
{
    public sealed class ItemReadRepository(FargoContext context) : EntityByGuidTemporalReadRepository<Item>(context.Items), IItemReadRepository
    {
        public async Task<IEnumerable<Item>> GetManyAsync(
            Guid? parentItemGuid = null,
            Guid? articleGuid = null,
            DateTime? atDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetManyAsync(
                atDateTime is not null ? dbSet.TemporalAsOf(atDateTime.Value) : dbSet.AsQueryable(),
                parentItemGuid,
                articleGuid,
                pagination, 
                cancellationToken);

        private static async Task<IEnumerable<Item>> GetManyAsync(
            IQueryable<Item> query,
            Guid? parentItemGuid = null,
            Guid? articleGuid = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetManyAsync(
                query.Where(x => parentItemGuid == null || x.ParentItemGuid == parentItemGuid && articleGuid == null || x.ArticleGuid == articleGuid),
                pagination,
                cancellationToken);
    }
}