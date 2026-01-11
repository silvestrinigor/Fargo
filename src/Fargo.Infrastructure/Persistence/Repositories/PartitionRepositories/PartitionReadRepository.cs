using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.PartitionRepositories;

namespace Fargo.Infrastructure.Persistence.Repositories.PartitionRepositories
{
    public class PartitionReadRepository(FargoContext context) : EntityByGuidTemporalReadRepository<Partition>(context.Partitions), IPartitionReadRepository;
}
