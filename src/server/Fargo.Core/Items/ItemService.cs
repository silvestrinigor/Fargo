using Fargo.Core.Common;

namespace Fargo.Core.Items;

/// <summary>
/// Provides domain operations and validation rules for item entities.
/// </summary>
/// <remarks>
/// This service contains business rules that require repository access and
/// therefore cannot be enforced by the <see cref="Item"/> entity alone.
/// </remarks>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <summary>
    /// Validates that assigning the specified parent container item to the specified
    /// item would result in a valid item containment hierarchy.
    /// </summary>
    /// <param name="parentContainerItem">
    /// The item container that will become the parent.
    /// </param>
    /// <param name="memberItem">
    /// The item that will become contained by the parent container.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentContainerItem"/> or
    /// <paramref name="memberItem"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the assignment would create a circular item containment hierarchy.
    /// </exception>
    /// <remarks>
    /// This method should be called before
    /// <see cref="Item.PlaceInsideContainer(Item)"/> because validating the
    /// complete containment hierarchy requires access to other items through the
    /// repository.
    /// </remarks>
    public async Task ValidateParentItemContainerHierarchyAssignmentAsync(
        Item parentContainerItem,
        Item memberItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);

        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentContainerItem, memberItem.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be assigned to parent container '{parentContainerItem.Guid}' because this would create a circular containment hierarchy.",
                FargoErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Determines whether assigning the specified item container as a parent
    /// would create a circular containment hierarchy.
    /// </summary>
    /// <param name="candidateParentContainer">
    /// The item container that would become the parent.
    /// </param>
    /// <param name="memberItemGuid">
    /// The identifier of the item receiving the new parent container.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the assignment would create a circular
    /// containment hierarchy; otherwise, <see langword="false"/>.
    /// </returns>
    private async Task<bool> CreatesCircularHierarchyAsync(
        Item candidateParentContainer,
        Guid memberItemGuid,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentContainer);

        if (candidateParentContainer.Guid == memberItemGuid)
        {
            return true;
        }

        var descendantItemGuids =
            await itemRepository.GetContainedDescendantGuidsAsync(
                memberItemGuid, false, cancellationToken);

        return descendantItemGuids.Contains(candidateParentContainer.Guid);
    }
}
