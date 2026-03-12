using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class PartitionRepository(FargoWriteDbContext context)
        : IPartitionRepository
    {
        private readonly DbSet<Partition> partitions = context.Partitions;

        public void Add(Partition partition)
            => partitions.Add(partition);

        public void Remove(Partition partition)
            => partitions.Remove(partition);

        public async Task<Partition?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .Where(x => x.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }
}