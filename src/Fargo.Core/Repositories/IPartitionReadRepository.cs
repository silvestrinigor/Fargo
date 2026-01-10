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

        Task<IEnumerable<Partition>> GetManyAsync(
            DateTime? atDateTime = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default
            );
    }
}
