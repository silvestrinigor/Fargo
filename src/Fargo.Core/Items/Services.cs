using Fargo.Core.Identity;

namespace Fargo.Core.Items;

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
        Actor actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);
        ArgumentNullException.ThrowIfNull(actor);

        memberItem.ValidateCanEdit(actor);
        actor.ValidateHasAccess(parentContainerItem);

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
        memberItem.MarkAsEditedBy(actor.Guid);
        memberItem.MarkModificationType(ItemModifiedType.ParentContainerChanged);
    }

    /// <summary>
    /// Clears the parent container relationship, leaving the item outside any container.
    /// </summary>
    public static void RemoveFromContainer(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.ParentContainer = null;
    }

    /// <summary>
    /// Clears the parent container relationship, leaving the item outside any container.
    /// </summary>
    public static void RemoveFromContainer(Item item, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(actor);

        item.ValidateCanEdit(actor);

        if (item.ParentContainerGuid is null)
        {
            return;
        }

        item.ParentContainer = null;
        item.MarkAsEditedBy(actor.Guid);
        item.MarkModificationType(ItemModifiedType.ParentContainerChanged);
    }
}
