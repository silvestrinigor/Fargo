using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;

namespace Fargo.Application.Repositories
{
    public interface IItemReadRepository
        {
            Task<ItemReadModel?> GetByGuid(
                    Guid entityGuid,
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<ItemReadModel>> GetMany(
                    IReadOnlyCollection<Guid> partitionGuids,
                    Guid? parentItemGuid = null,
                    Guid? articleGuid = null,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                );
        }
}