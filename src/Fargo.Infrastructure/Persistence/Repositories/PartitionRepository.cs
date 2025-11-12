using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class PartitionRepository : IPartitionRepository
{
    public void Add(Partition entity)
    {
        throw new NotImplementedException();
    }

    public Task<Partition?> GetAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Partition>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Entity>> GetContainerPartition(Guid containerGuid)
    {
        throw new NotImplementedException();
    }

    public Task<Container?> GetEntityPartition(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Guid>> GetGuidsAsync()
    {
        throw new NotImplementedException();
    }

    public void Remove(Partition entity)
    {
        throw new NotImplementedException();
    }
}
