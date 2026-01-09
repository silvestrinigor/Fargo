using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IPartitionReadRepository
    {
        Task<Partition?> GetByGuidAsync(
            Guid partitionGuid,
            DateTime? atDateTime = null,
            CancellationToken cancellationToken = default
            );
    }
}
