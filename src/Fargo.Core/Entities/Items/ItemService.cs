using Fargo.Core.Common;

namespace Fargo.Core.Entities.Items;

/// <summary>
/// Item rules that need repository access and so cannot be enforced by <see cref="Item"/>.
/// </summary>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <remarks>
    /// Call before <see cref="Item.PlaceInsideContainer(Item)"/>, which cannot detect indirect
    /// cycles because that requires reading other items.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The assignment would create a circular containment hierarchy.
    /// </exception>
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
    /// Checks the member item against the container article's allow-list and per-article maximum
    /// quantity.
    /// </summary>
    /// <remarks>
    /// No check is done when the container defines no allow-list. The container article and its
    /// allow-list (<c>Article.Container.AllowedArticles</c>) must be loaded on
    /// <paramref name="parentContainerItem"/>; the repository is only used to count the article's
    /// existing direct contents when a maximum quantity applies.
    /// </remarks>
    /// <exception cref="FargoCoreException">
    /// The member's article is not in the allow-list, or placing the item would exceed the
    /// article's maximum quantity for the container.
    /// </exception>
    public async Task ValidateAllowedInContainerAsync(
        Item parentContainerItem,
        Item memberItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentContainerItem);
        ArgumentNullException.ThrowIfNull(memberItem);

        var container = parentContainerItem.Article.Container;

        // A non-container parent is rejected in Item.PlaceInsideContainer; guard defensively here.
        if (container is null)
        {
            return;
        }

        var allowedArticles = container.AllowedArticles;

        if (allowedArticles.Count == 0)
        {
            return;
        }

        var allowed = allowedArticles.FirstOrDefault(a => a.AllowedArticleGuid == memberItem.ArticleGuid) ?? throw new FargoCoreException(
                $"Item '{memberItem.Guid}' cannot be placed inside container '{parentContainerItem.Guid}' because its article '{memberItem.ArticleGuid}' is not in the container's allowed articles.",
                FargoErrorType.InvalidOperation);
        if (allowed.MaxQuantity is not { } maxQuantity)
        {
            return;
        }

        // The item being placed is not counted as a direct child yet, so add one for it.
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
