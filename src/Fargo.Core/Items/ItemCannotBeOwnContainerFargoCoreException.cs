namespace Fargo.Core.Items;

/// <summary>
/// Exception thrown when an item is placed inside itself.
/// </summary>
public sealed class ItemCannotBeOwnContainerFargoCoreException(Guid itemGuid)
    : FargoCoreException(
        $"Item '{itemGuid}' cannot be its own container.",
        FargoCoreErrorType.ItemCannotBeOwnContainer)
{
    /// <summary>
    /// Gets the identifier of the item involved in the violation.
    /// </summary>
    public Guid ItemGuid { get; } = itemGuid;
}
