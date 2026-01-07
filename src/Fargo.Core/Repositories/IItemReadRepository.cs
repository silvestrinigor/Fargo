using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Repositories
{
    public interface IItemReadRepository
    {
        Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default);

        Task<IEnumerable<Item>> GetManyAsync(Guid? ArticleGuid = null, CancellationToken cancellationToken = default);
    }
}
