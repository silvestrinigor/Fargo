using Fargo.Core.Common;

namespace Fargo.Core.Entities.Items;

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
    /// Validates that the specified member item is allowed to be placed inside
    /// the specified container item, according to the container article's
    /// allow-list and per-article maximum quantity.
    /// </summary>
    /// <param name="parentContainerItem">
    /// The container item that will become the parent.
    /// </param>
    /// <param name="memberItem">
    /// The item that will be placed inside the container.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentContainerItem"/> or
    /// <paramref name="memberItem"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the container defines an allow-list and the member item's
    /// article is not on it, or when placing the item would exceed the allowed
    /// maximum quantity of its article for the container.
    /// </exception>
    /// <remarks>
    /// When the container article defines no allow-list (the collection is
    /// empty), any article may be placed and this method performs no check.
    ///
    /// The container article and its allow-list must be loaded on
    /// <paramref name="parentContainerItem"/> (<c>Article.Container.AllowedArticles</c>);
    /// the repository is only consulted to count the article's existing direct
    /// contents when a maximum quantity applies.
    /// </remarks>
    public async Task ValidateAllowedInContainerAsync(
        Item parentContainerItem,
        Item memberItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);

        var container = parentContainerItem.Article.Container;

        // Only container articles carry an allow-list. A non-container parent is
        // rejected elsewhere (Item.PlaceInsideContainer); guard defensively here.
        if (container is null)
        {
            return;
        }

        var allowedArticles = container.AllowedArticles;

        // An empty allow-list means the container is unconstrained: any article
        // may be placed inside it.
        if (allowedArticles.Count == 0)
        {
            return;
        }

        var allowed = allowedArticles.FirstOrDefault(a => a.AllowedArticleGuid == memberItem.ArticleGuid);

        if (allowed is null)
        {
            throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be placed inside container '{parentContainerItem.Guid}' because its article '{memberItem.ArticleGuid}' is not in the container's allowed articles.",
                FargoErrorType.InvalidOperation);
        }

        if (allowed.MaxQuantity is not { } maxQuantity)
        {
            return;
        }

        // Count the article's items already directly inside the container. The
        // item being placed is not yet (or no longer) counted as a direct child
        // of this container, so adding one accounts for it.
        var currentCount = await itemRepository.CountDirectChildrenByArticleAsync(
            parentContainerItem.Guid, memberItem.ArticleGuid, cancellationToken);

        var resultingCount = currentCount + 1;

        if (resultingCount > maxQuantity.Value)
        {
            throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be placed inside container '{parentContainerItem.Guid}' because it would exceed the maximum allowed quantity ({maxQuantity.Value}) of article '{memberItem.ArticleGuid}' for the container.",
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
