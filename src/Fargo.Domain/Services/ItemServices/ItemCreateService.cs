using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.ItemServices
{
    public class ItemCreateService(
            IItemRepository itemRepository
            )
    {
        public Item CreateItem(
                User actor,
                Article itemArticle
                )
        {
            actor.ValidatePermission(ActionType.CreateItem);

            var item = new Item
            {
                Article = itemArticle,
                UpdatedBy = actor
            };

            itemRepository.Add(item);

            return item;
        }
    }
}