using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public sealed class ItemReadRepository(FargoReadDbContext context) : EntityByGuidTemporalReadRepository<ItemReadModel>(context.Items), IItemReadRepository
    {
        public async Task<IEnumerable<ItemReadModel>> GetManyAsync(
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

        private static async Task<IEnumerable<ItemReadModel>> GetManyAsync(
            IQueryable<ItemReadModel> query,
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