using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IItemReadRepository
    {
        Task<Item?> GetByGuidAsync(Guid itemGuid, DateTime? atDateTime = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<Item>> GetManyAsync(Guid? articleGuid = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default);
    }
}
