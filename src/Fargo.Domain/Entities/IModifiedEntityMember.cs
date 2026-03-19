namespace Fargo.Domain.Entities;

/// <summary>
/// Defines a contract for entities that belong to an audited aggregate
/// and whose modifications must update the audit metadata of the aggregate root.
/// </summary>
/// <remarks>
/// Implementations of this interface indicate that any change to the current
/// entity should propagate a modification update to a parent entity that
/// implements <see cref="IModifiedEntity"/>.
///
/// This is typically used for aggregate members whose lifecycle is controlled
/// by an audited aggregate root, ensuring that changes within the aggregate
/// are consistently reflected in the root's modification metadata.
/// </remarks>
public interface IModifiedEntityMember
{
    /// <summary>
    /// Gets the parent entity whose modification metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// The returned entity must implement <see cref="IModifiedEntity"/> and is
    /// expected to have its audit state updated (e.g., via
    /// <see cref="IModifiedEntity.MarkAsEdited(Guid)"/>) whenever this member
    /// is modified.
    /// </remarks>
    IModifiedEntity ParentEditedEntity { get; }
}
