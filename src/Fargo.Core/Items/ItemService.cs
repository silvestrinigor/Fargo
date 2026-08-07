namespace Fargo.Core.Items;

/// <summary>
/// Provides domain operations and validation rules for <see cref="Item"/> entities.
/// </summary>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <summary>
    /// Ensures that the specified container can be assigned as the parent of the specified item.
    /// </summary>
    /// <param name="parentContainerItem">
    /// The candidate parent container.
    /// </param>
    /// <param name="memberItem">
    /// The item whose parent container is being assigned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either argument is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown if the destination item is not a container, if the item is assigned
    /// to itself, or if the assignment would create a circular hierarchy.
    /// </exception>
    public async Task ValidateParentItemContainerAssignmentAsync(
        Item parentContainerItem,
        Item memberItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);

        var descendantItemGuids = await itemRepository.GetContainerDescendantGuidsAsync(
            memberItem.Guid,
            includeRoot: false,
            cancellationToken);

        if (descendantItemGuids.Contains(parentContainerItem.Guid))
        {
            throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be assigned to container '{parentContainerItem.Guid}' because this would create a circular hierarchy.",
                FargoCoreErrorType.InvalidOperation);
        }
    }
}
