using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IItemRepository
    {
        Task<Item?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        void Add(Item item);

        void Remove(Item item);
    }
}