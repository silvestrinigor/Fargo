using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public sealed class ItemReadRepository(FargoReadDbContext context)
        : EntityByGuidTemporalPartitionedReadRepository<ItemReadModel>(context.Items), IItemReadRepository
    {
        public async Task<IEnumerable<ItemReadModel>> GetManyAsync(
                IReadOnlyCollection<Guid> partitionGuids,
                Guid? parentItemGuid = null,
                Guid? articleGuid = null,
                DateTime? TemporalAsOf = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await GetManyAsync(
                    TemporalAsOf is not null ? dbSet.TemporalAsOf(TemporalAsOf.Value) : dbSet.AsQueryable(),
                    partitionGuids,
                    parentItemGuid,
                    articleGuid,
                    pagination,
                    cancellationToken
                    );

        private static async Task<IEnumerable<ItemReadModel>> GetManyAsync(
                IQueryable<ItemReadModel> query,
                IReadOnlyCollection<Guid> partitionGuids,
                Guid? parentItemGuid = null,
                Guid? articleGuid = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default)
            => await GetManyAsync(
                    query.Where(x =>
                        parentItemGuid == null ||
                        x.ParentItemGuid == parentItemGuid && articleGuid == null ||
                        x.ArticleGuid == articleGuid
                        ),
                    pagination,
                    cancellationToken
                    );
    }
}