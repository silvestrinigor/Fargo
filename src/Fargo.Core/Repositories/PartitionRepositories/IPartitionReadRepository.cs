using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.PartitionRepositories
{
    public interface IPartitionReadRepository : IEntityByGuidTemporalReadRepository<Partition>;
}
