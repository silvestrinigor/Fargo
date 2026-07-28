namespace Fargo.Core.Items;

/// <summary>
/// Exception thrown when an item container hierarchy would become circular.
/// </summary>
public sealed class ItemCircularContainerHierarchyFargoCoreException(
    Guid parentContainerItemGuid,
    Guid memberItemGuid)
    : FargoCoreException(
        $"Item '{memberItemGuid}' cannot be assigned to container " +
        $"'{parentContainerItemGuid}' because this would create a circular hierarchy.",
        FargoCoreErrorType.ItemCircularContainerHierarchy)
{
    /// <summary>
    /// Gets the identifier of the candidate parent container item.
    /// </summary>
    public Guid ParentContainerItemGuid { get; } = parentContainerItemGuid;

    /// <summary>
    /// Gets the identifier of the member item.
    /// </summary>
    public Guid MemberItemGuid { get; } = memberItemGuid;
}
