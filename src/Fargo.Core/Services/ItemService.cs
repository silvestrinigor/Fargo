using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.ItemRepositories;

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

        public async Task InsertItemIntoContainerAsync(Item item, Item targetContainer)
        {
            if (!targetContainer.Article.IsContainer)
                throw new InvalidOperationException(
                    "Cannot be inserted into an item that is not a container.");

            if (item.Guid == targetContainer.Guid)
                throw new InvalidOperationException(
                    "An item cannot be moved into itself.");

            if (await itemRepository.IsInsideContainer(item, targetContainer))
                throw new InvalidOperationException(
                    "An item cannot be moved inside a container when the container is inside the item.");

            item.ParentItem = targetContainer;
        }

        public static void RemoveFromContainers(Item item)
        {
            item.ParentItem = null;
        }
    }
}
