using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IPartitionRepository : IEntityByGuidRepository<Partition>
    {
        void Add(Partition paritition);

        void Remove(Partition paritition);
    }
}
