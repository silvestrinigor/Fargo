using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.ItemRepositories
{
    public interface IItemRepository : IEntityByGuidRepository<Item>
    {
        Task<bool> IsInsideContainer(
            Item item, 
            Item container,
            CancellationToken cancellationToken = default);

        void Add(Item item);

        void Remove(Item item);
    }
}
