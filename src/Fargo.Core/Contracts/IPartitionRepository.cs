using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Contracts;

public interface IPartitionRepository : IEntityRepository<Partition>
{
    Task<Container?> GetEntityPartition(Guid guid);
    Task<IEnumerable<Entity>> GetContainerPartition(Guid containerGuid);
}
