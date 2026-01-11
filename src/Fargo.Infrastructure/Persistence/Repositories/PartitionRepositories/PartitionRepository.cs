using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.PartitionRepositories;

namespace Fargo.Infrastructure.Persistence.Repositories.PartitionRepositories
{
    public class PartitionRepository(FargoContext context) : EntityByGuidRepository<Partition>(context.Partitions), IPartitionRepository
    {
        private readonly FargoContext context = context;

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
