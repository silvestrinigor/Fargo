using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;

namespace Fargo.Application.Repositories
{
    public interface IItemReadRepository
        : IEntityByGuidTemporalPartitionedReadRepository<ItemReadModel>
        {
            Task<IEnumerable<ItemReadModel>> GetManyAsync(
                    IReadOnlyCollection<Guid> partitionGuids,
                    Guid? parentItemGuid = null,
                    Guid? articleGuid = null,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );
        }
}