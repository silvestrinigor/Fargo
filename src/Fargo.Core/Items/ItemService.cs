namespace Fargo.Core.Items;

public sealed class ItemService(IItemRepository itemRepository)
{
    public async Task MoveToContainer(Item parentContainerItem, Item memberItem, CancellationToken cancellationToken = default)
    {
        if (parentContainerItem.Guid == memberItem.Guid)
        {
            throw new FargoCoreException($"Item '{memberItem.Guid}' cannot be its own container.");
        }

        if (!parentContainerItem.Article.IsContainer)
        {
            throw new FargoCoreException($"Item '{parentContainerItem.Guid}' is not a container.");
        }

        var descendantItemGuids = await itemRepository.GetContainerDescendantGuids(
            memberItem.Guid,
            includeRoot: false,
            cancellationToken);

        if (descendantItemGuids.Contains(parentContainerItem.Guid))
        {
            throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be assigned to container " +
                $"'{parentContainerItem.Guid}' because this would create a circular hierarchy.");
        }

        memberItem.ParentContainer = parentContainerItem;
    }

    public static void RemoveFromContainer(Item item)
    {
        item.ParentContainer = null;
    }
}
