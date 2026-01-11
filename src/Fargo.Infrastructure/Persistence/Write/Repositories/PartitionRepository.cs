using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public class PartitionRepository(FargoWriteDbContext context) : EntityByGuidRepository<Partition>(context.Partitions), IPartitionRepository
    {
        private readonly FargoWriteDbContext context = context;

        public void Add(Partition paritition)
        {
            context.Add(paritition);
        }

        public void Remove(Partition paritition)
        {
            context.Remove(paritition);
        }
    }
}
