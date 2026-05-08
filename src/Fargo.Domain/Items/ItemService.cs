namespace Fargo.Domain.Items;

/// <summary>
/// Provides domain operations for moving items between item containers.
/// </summary>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <summary>
    /// Places an item inside a container item.
    /// </summary>
    /// <exception cref="ItemCannotBeOwnContainerFargoDomainException">
    /// Thrown when an item is assigned as its own container.
    /// </exception>
    /// <exception cref="ItemParentIsNotContainerFargoDomainException">
    /// Thrown when the parent item is not backed by a container article.
    /// </exception>
    /// <exception cref="ItemCircularContainerHierarchyFargoDomainException">
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
            throw new ItemCannotBeOwnContainerFargoDomainException(memberItem.Guid);
        }

        if (!parentContainerItem.Article.IsContainer)
        {
            throw new ItemParentIsNotContainerFargoDomainException(parentContainerItem.Guid);
        }

        var descendantItemGuids = await itemRepository.GetContainerDescendantGuids(
            memberItem.Guid,
            includeRoot: false,
            cancellationToken);

        if (descendantItemGuids.Contains(parentContainerItem.Guid))
        {
            throw new ItemCircularContainerHierarchyFargoDomainException(
                parentContainerItem.Guid,
                memberItem.Guid);
        }

        memberItem.ParentContainer = new ItemContainer(parentContainerItem);
    }

    /// <summary>
    /// Removes an item from its current parent container.
    /// </summary>
    public static void RemoveFromContainer(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.ParentContainer = null;
    }
}
