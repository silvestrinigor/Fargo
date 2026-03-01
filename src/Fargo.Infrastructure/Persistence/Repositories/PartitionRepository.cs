using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class PartitionRepository(FargoWriteDbContext context) : IPartitionRepository
    {
        private readonly DbSet<Partition> partitions = context.Partitions;

        public void Add(Partition paritition)
        {
            context.Add(paritition);
        }

        public async Task<Partition?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                )
            => await partitions
            .Where(a =>
                    a.Guid == entityGuid
                  )
            .SingleOrDefaultAsync(cancellationToken);

        public void Remove(Partition paritition)
        {
            context.Remove(paritition);
        }
    }
}