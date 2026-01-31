using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ItemService(IItemRepository itemRepository)
    {
        private readonly IItemRepository itemRepository = itemRepository;

        public async Task<Item> GetItemAsync(
                Guid itemGuid, 
                CancellationToken cancellationToken = default)
            => await itemRepository.GetByGuidAsync(itemGuid, cancellationToken)
            ?? throw new ItemNotFoundException(itemGuid);

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
            => itemRepository.Remove(item);

        public async Task InsertItemIntoContainerAsync(Item item, Item targetContainer)
        {
            if (await itemRepository.IsInsideContainer(item, targetContainer))
                throw new ItemParentInsideItemException(item, targetContainer);

            item.ParentItem = targetContainer;
        }

        public static void RemoveFromContainers(Item item)
        {
            item.ParentItem = null;
        }
    }
}