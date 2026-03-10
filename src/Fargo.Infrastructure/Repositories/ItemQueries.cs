using Fargo.Application.Common;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public sealed class ItemQueries(FargoReadDbContext context)
        : IItemQueries
    {
        private readonly DbSet<ItemReadModel> items = context.Items;

        public async Task<ItemReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfProvided(asOfDateTime)
            .Where(a =>a.Guid == entityGuid)
            .OrderBy(x => x.Guid)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<ItemReadModel>> GetMany(
                Pagination pagination,
                Guid? articleGuid = null,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await items
            .TemporalAsOfIfProvided(asOfDateTime)
            .Where(i => articleGuid == null || i.ArticleGuid == articleGuid)
            .OrderBy(x => x.Guid)
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}