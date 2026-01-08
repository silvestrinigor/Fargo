using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ItemService(IItemRepository itemRepository)
    {
        private readonly IItemRepository itemRepository = itemRepository;

        public async Task<Item?> GetItemAsync(Guid itemGuid, CancellationToken cancellationToken = default)
        {
            return await itemRepository.GetByGuidAsync(itemGuid, cancellationToken);
        }

        public Item CreateItem(Article article)
        {
            var item = new Item
            {
                Article = article
            };

            itemRepository.Add(item);

            return item;
        }

        public void DeleteItem(Item item)
        {
            itemRepository.Remove(item);
        }
    }
}
