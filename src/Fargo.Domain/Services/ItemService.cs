using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public class ItemService(IItemRepository itemRepository)
    {
        public async Task<Item?> GetItem(
                Actor actor,
                Guid itemGuid,
                CancellationToken cancellationToken = default
                )
            => await itemRepository.GetByGuid(
                    itemGuid,
                    actor.PartitionGuids,
                    cancellationToken
                    );

        public Item CreateItem(Actor actor, Article article)
        {
            if (!actor.HasPermission(ActionType.CreateItem))
                throw new ActorNotAuthorizedException(
                        actor,
                        ActionType.CreateItem
                        );

            var item = new Item
            {
                Article = article
            };

            itemRepository.Add(item);

            return item;
        }

        public void DeleteItem(Actor actor, Item item)
        {
            if (!actor.HasPermission(ActionType.DeleteItem))
                throw new ActorNotAuthorizedException(actor, ActionType.DeleteItem);

            itemRepository.Remove(item);
        }

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