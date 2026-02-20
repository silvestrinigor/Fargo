using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class PartitionReadRepository(FargoReadDbContext context)
        : IPartitionReadRepository
    {
        private readonly DbSet<PartitionReadModel> partitions = context.Partitions;

        public async Task<PartitionReadModel?> GetByGuid(
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<PartitionReadModel>> GetMany(
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a => a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}