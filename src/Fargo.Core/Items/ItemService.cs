namespace Fargo.Core.Items;

/// <summary>
/// Provides domain operations for moving items between item containers.
/// </summary>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <summary>
    /// Places an item inside a container item.
    /// </summary>
    /// <exception cref="ItemCannotBeOwnContainerFargoCoreException">
    /// Thrown when an item is assigned as its own container.
    /// </exception>
    /// <exception cref="ItemIsNotContainerFargoCoreException">
    /// Thrown when the parent item is not backed by a container article.
    /// </exception>
    /// <exception cref="ItemCircularContainerHierarchyFargoCoreException">
    /// Thrown when assigning the container would create a circular hierarchy.
    /// </exception>
    public async Task MoveToContainer(
        Item parentContainerItem,
        Item memberItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);

        if (parentContainerItem.Guid == memberItem.Guid)
        {
            throw new ItemCannotBeOwnContainerFargoCoreException(memberItem.Guid);
        }

        if (!parentContainerItem.Article.IsContainer)
        {
            throw new ItemIsNotContainerFargoCoreException(parentContainerItem.Guid);
        }

        var descendantItemGuids = await itemRepository.GetContainerDescendantGuids(
            memberItem.Guid,
            includeRoot: false,
            cancellationToken);

        if (descendantItemGuids.Contains(parentContainerItem.Guid))
        {
            throw new ItemCircularContainerHierarchyFargoCoreException(
                parentContainerItem.Guid,
                memberItem.Guid);
        }

        memberItem.ParentContainer = new ItemContainer(parentContainerItem);
    }

    /// <summary>
    /// Clears the parent container relationship, leaving the item outside any container.
    /// </summary>
    public static void RemoveFromContainer(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.ParentContainer = null;
    }
}
