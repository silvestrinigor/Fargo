using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IPartitionRepository
    {
        Task<Partition?> GetByGuidAsync(
            Guid partitionGuid,
            CancellationToken cancellationToken = default
            );

        void Add(Partition paritition);

        void Remove(Partition paritition);
    }
}
