using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Events;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ItemService
    {
        private readonly IItemRepository itemRepository;

        private readonly IEventRepository eventRepository;

        public ItemService(IItemRepository itemRepository, IEventRepository eventRepository)
        {
            this.itemRepository = itemRepository;
            this.eventRepository = eventRepository;

            ItemCreated += HandleItemCreated;
            ItemDeleted += HandleItemDeleted;
        }

        public async Task<Item?> GetItem(Guid itemGuid, CancellationToken cancellationToken = default)
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

            OnItemCreated(item);

            return item;
        }

        public event EventHandler<ItemCreatedEventArgs>? ItemCreated;

        private void OnItemCreated(Item item)
        {
            ItemCreated?.Invoke(this, new ItemCreatedEventArgs(item));
        }

        private void HandleItemCreated(object? sender, ItemCreatedEventArgs e)
        {
            var newEvent = new Event(e);

            eventRepository.Add(newEvent);
        }

        public void DeleteItem(Item item)
        {
            itemRepository.Remove(item);

            OnItemDeleted(item);
        }

        public event EventHandler<ItemDeletedEventArgs>? ItemDeleted;

        private void OnItemDeleted(Item item)
        {
            ItemDeleted?.Invoke(this, new ItemDeletedEventArgs(item));
        }

        private void HandleItemDeleted(object? sender, ItemDeletedEventArgs e)
        {
            var newEvent = new Event(e);

            eventRepository.Add(newEvent);
        }
    }
}
