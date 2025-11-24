using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories;

public interface IPartitionRepository : IEntityRepository<Partition>
{
    Task<Container?> GetEntityPartition(Guid guid);
    Task<IEnumerable<NamedEntity>> GetContainerPartition(Guid containerGuid);
}
