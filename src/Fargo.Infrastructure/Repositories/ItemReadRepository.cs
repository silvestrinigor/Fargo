using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public sealed class ItemReadRepository(FargoReadDbContext context)
        : IItemReadRepository
    {
        private readonly DbSet<ItemReadModel> items = context.Items;

        public async Task<ItemReadModel?> GetByGuid(
                Guid entityGuid,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<ItemReadModel>> GetMany(
                Guid? parentItemGuid = null,
                Guid? articleGuid = null,
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(i =>
                    (articleGuid == null || i.ArticleGuid == articleGuid))
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}