using Fargo.Domain.Entities.Itens;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    public class ItemService(IItemRepository itemRepository)
    {
        private readonly IItemRepository itemRepository = itemRepository;

        public Item CreateArticle(Item item)
        {
            itemRepository.Add(item);
            OnItemCreated();
            return item;
        }

        public event EventHandler? ItemCreated;

        private void OnItemCreated()
        {
            ItemCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}
