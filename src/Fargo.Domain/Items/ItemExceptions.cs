namespace Fargo.Domain.Items;

/// <summary>
/// Exception thrown when an item is placed inside itself.
/// </summary>
public sealed class ItemCannotBeOwnContainerFargoDomainException(Guid itemGuid)
    : FargoDomainException($"Item '{itemGuid}' cannot be its own container.")
{
    /// <summary>
    /// Gets the identifier of the item involved in the violation.
    /// </summary>
    public Guid ItemGuid { get; } = itemGuid;
}

/// <summary>
/// Exception thrown when an item is placed inside a non-container item.
/// </summary>
public sealed class ItemParentIsNotContainerFargoDomainException(Guid parentItemGuid)
    : FargoDomainException($"Item '{parentItemGuid}' cannot contain items.")
{
    /// <summary>
    /// Gets the identifier of the invalid parent item.
    /// </summary>
    public Guid ParentItemGuid { get; } = parentItemGuid;
}

/// <summary>
/// Exception thrown when an item container hierarchy would become circular.
/// </summary>
public sealed class ItemCircularContainerHierarchyFargoDomainException(
    Guid parentContainerItemGuid,
    Guid memberItemGuid)
    : FargoDomainException(
        $"Item '{memberItemGuid}' cannot be assigned to container " +
        $"'{parentContainerItemGuid}' because this would create a circular hierarchy.")
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
