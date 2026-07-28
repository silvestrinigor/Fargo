namespace Fargo.Core.Items;

/// <summary>
/// Exception thrown when an item is not a container.
/// </summary>
public sealed class ItemIsNotContainerFargoCoreException(Guid itemGuid)
    : FargoCoreException(
        $"Item '{itemGuid}' is not a container.",
        FargoCoreErrorType.ItemIsNotContainer)
{
    /// <summary>
    /// Gets the identifier of the item.
    /// </summary>
    public Guid ItemGuid { get; } = itemGuid;
}
