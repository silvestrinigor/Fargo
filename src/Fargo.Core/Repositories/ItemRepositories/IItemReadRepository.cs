using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories.ItemRepositories
{
    public interface IItemReadRepository : IEntityByGuidTemporalReadRepository<Item>
    {
        Task<IEnumerable<Item>> GetManyAsync(
            Guid? parentItemGuid = null,
            Guid? articleGuid = null,
            DateTime? atDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default);
    }
}
