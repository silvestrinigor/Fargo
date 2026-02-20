using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IPartitionRepository
    {
        Task<Partition?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        void Add(Partition paritition);

        void Remove(Partition paritition);
    }
}