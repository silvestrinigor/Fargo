using Fargo.Application.Common;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class PartitionQueries(FargoReadDbContext context) : IPartitionQueries
    {
        private readonly DbSet<PartitionReadModel> partitions = context.Partitions;

        public async Task<PartitionReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .Where(x => x.Guid == entityGuid)
            .AsNoTracking()
            .OrderBy(x => x.Guid)
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<PartitionReadModel>> GetMany(
                Pagination pagination,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .OrderBy(x => x.Guid)
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}