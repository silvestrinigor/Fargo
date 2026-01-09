using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    internal class PartitionReadRepository(FargoContext context) : IPartitionReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<Partition?> GetByGuidAsync(
            Guid partitionGuid, 
            DateTime? atDateTime = null, 
            CancellationToken cancellationToken = default)
        {
            var query = atDateTime is not null
                ? context.Partitions.TemporalAsOf(atDateTime.Value)
                : context.Partitions;

            return await query
                .AsNoTracking()
                .Where(x => x.Guid == partitionGuid)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
