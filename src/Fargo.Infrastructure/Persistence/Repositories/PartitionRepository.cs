using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class PartitionRepository(FargoContext context) : IPartitionRepository
    {
        private readonly FargoContext context = context;

        public void Add(Partition paritition)
        {
            throw new NotImplementedException();
        }

        public async Task<Partition?> GetByGuidAsync(
            Guid partitionGuid,
            CancellationToken cancellationToken = default)
            => await context.Partitions
            .Where(x => x.Guid == partitionGuid)
            .SingleOrDefaultAsync(cancellationToken);

        public void Remove(Partition paritition)
        {
            context.Remove(paritition);
        }
    }
}
