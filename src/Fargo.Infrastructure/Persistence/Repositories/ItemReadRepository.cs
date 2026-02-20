using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public sealed class ItemReadRepository(FargoReadDbContext context)
        : IItemReadRepository
    {
        private readonly DbSet<ItemReadModel> items = context.Items;

        public async Task<ItemReadModel?> GetByGuid(
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<ItemReadModel>> GetMany(
                IReadOnlyCollection<Guid> partitionGuids,
                Guid? parentItemGuid = null,
                Guid? articleGuid = null,
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(i =>
                    i.Partitions.Any(p => partitionGuids.Contains(p.Guid)) &&
                    (parentItemGuid == null || i.ParentItemGuid == parentItemGuid) &&
                    (articleGuid == null || i.ArticleGuid == articleGuid))
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}