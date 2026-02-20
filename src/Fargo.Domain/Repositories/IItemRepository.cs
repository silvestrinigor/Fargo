using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IItemRepository
    {
        Task<Item?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        Task<bool> IsInsideContainer(
                Item item,
                Item container,
                CancellationToken cancellationToken = default
                );

        void Add(Item item);

        void Remove(Item item);
    }
}