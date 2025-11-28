using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IItemRepository
    {
        void Add(Item item);
        void Remove(Item item);
    }
}
