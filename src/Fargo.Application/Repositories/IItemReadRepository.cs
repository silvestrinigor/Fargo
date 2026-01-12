using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;

namespace Fargo.Application.Repositories
{
    public interface IItemReadRepository : IEntityByGuidTemporalReadRepository<ItemReadModel>
    {
        Task<IEnumerable<ItemReadModel>> GetManyAsync(
            Guid? parentItemGuid = null,
            Guid? articleGuid = null,
            DateTime? asOfDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default);
    }
}
