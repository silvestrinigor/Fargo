namespace Fargo.Core.Modifiables;

/// <summary>
/// Defines a contract for entities that belong to an audited aggregate
/// and whose modifications must update the audit metadata of the aggregate root.
/// </summary>
/// <remarks>
/// Implementations of this interface indicate that any change to the current
/// entity should propagate a modification update to a parent entity that
/// implements <see cref="IModifiable"/>.
///
/// This is typically used for aggregate members whose lifecycle is controlled
/// by an audited aggregate root, ensuring that changes within the aggregate
/// are consistently reflected in the root's modification metadata.
/// </remarks>
public interface IModifiableMember
{
    /// <summary>
    /// Gets the parent entity whose modification metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// The returned entity must implement <see cref="IModifiable"/> and is
    /// expected to have its audit state updated (e.g., via
    /// <see cref="IModifiable.MarkAsEditedBy(Guid)"/>) whenever this member
    /// is modified.
    /// </remarks>
    IModifiable ParentEditedEntity { get; }
}
