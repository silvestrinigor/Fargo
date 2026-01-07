using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Repositories
{
    public interface IItemRepository
    {
        Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default);
        
        void Add(Item item);
        
        void Remove(Item item);
    }
}
