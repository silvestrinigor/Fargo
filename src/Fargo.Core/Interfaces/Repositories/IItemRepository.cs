using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IItemRepository
    {
        Task<Item?> GetByGuidAsync(Guid guid);
        void Add(Item item);
        void Remove(Item item);
    }
}
