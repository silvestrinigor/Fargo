using Fargo.Domain.Entities.ArticleItems;

namespace Fargo.Domain.Repositories
{
    public interface IItemReadRepository
    {
        Task<Item?> GetByGuidAsync(
            Guid itemGuid, 
            DateTime? atDateTime = null, 
            CancellationToken cancellationToken = default
            );

        Task<IEnumerable<Item>> GetManyAsync(
            Guid? parentItemGuid = null,
            Guid? articleGuid = null, 
            DateTime? atDateTime = null, 
            int? skip = null, 
            int? take = null, 
            CancellationToken cancellationToken = default
            );
    }
}
