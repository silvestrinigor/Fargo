using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Repositories
{
    public interface IPartitionReadRepository
    {
            Task<PartitionReadModel?> GetByGuid(
                    Guid entityGuid,
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<PartitionReadModel>> GetMany(
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );
    }
}