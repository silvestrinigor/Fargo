using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.ItemServices
{
    public class ItemDeleteService(
            IItemRepository itemRepository
            )
    {
        public void DeleteItem(
                User actor,
                Item item
                )
        {
            actor.ValidatePermission(ActionType.DeleteItem);

            itemRepository.Remove(item);
        }
    }
}