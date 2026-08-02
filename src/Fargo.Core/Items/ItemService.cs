using Fargo.Core.Shared.Articles;

namespace Fargo.Core.Items;

/// <summary>
/// Item core service.
/// </summary>
public sealed class ItemService(IItemRepository itemRepository)
{
    /// <summary>
    /// Moves an item into the specified container.
    /// </summary>
    /// <param name="parentContainerItem">The destination container item.</param>
    /// <param name="memberItem">The item to move.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown if the destination is not a container, if the item is assigned to
    /// itself, or if the operation would create a circular container hierarchy.
    /// </exception>
    public async Task MoveToContainerAsync(Item parentContainerItem, Item memberItem, CancellationToken cancellationToken = default)
    {
        if (parentContainerItem.Guid == memberItem.Guid)
        {
            throw new FargoCoreException($"Item '{memberItem.Guid}' cannot be assigned to itself as a container.");
        }

        if (parentContainerItem.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException($"Item '{parentContainerItem.Guid}' is not a container item.");
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

        memberItem.SetParentItemContainer(parentContainerItem);
    }
}
