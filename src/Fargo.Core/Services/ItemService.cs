using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Events;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ItemService(IItemRepository itemRepository, IEventRepository eventRepository)
    {
        private readonly IItemRepository itemRepository = itemRepository;

        private readonly IEventRepository eventRepository = eventRepository;

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

            var newEvent = new ModelCreatedEvent(item);

            eventRepository.Add(newEvent);

            OnItemCreated(item);

            return item;
        }

        public event EventHandler<ItemCreatedEventArgs>? ItemCreated;

        private void OnItemCreated(Item item)
        {
            ItemCreated?.Invoke(this, new ItemCreatedEventArgs(item));
        }

        public void DeleteItem(Item item)
        {
            itemRepository.Remove(item);

            var newEvent = new ModelDeletedEvent(item);

            eventRepository.Add(newEvent);

            OnItemDeleted(item);
        }

        public event EventHandler<ItemDeletedEventArgs>? ItemDeleted;

        private void OnItemDeleted(Item item)
        {
            ItemDeleted?.Invoke(this, new ItemDeletedEventArgs(item));
        }
    }
}
