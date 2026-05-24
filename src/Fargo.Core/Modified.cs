namespace Fargo.Core;

/// <summary>
/// Defines the contract for entities that track last modification metadata.
/// </summary>
/// <remarks>
/// Implementations of this interface expose information about the last
/// modification performed on the entity, including the actor responsible
/// for the change.
/// Command handlers are responsible for assigning the modification metadata
/// when they apply a change.
/// </remarks>
public interface IModifiedEntity
{
    /// <summary>
    /// Gets the unique identifier of the actor that last modified the entity.
    /// </summary>
    /// <remarks>
    /// This value is <see langword="null"/> when the entity has not
    /// been modified since its creation.
    ///
    /// When the modification is performed by an internal system process,
    /// callers should pass the actor guid that represents that process.
    /// </remarks>
    Guid? EditedByGuid { get; }

    /// <summary>
    /// Marks the entity as edited by the specified actor.
    /// </summary>
    /// <param name="actorGuid">
    /// The unique identifier of the actor performing the modification.
    /// </param>
    /// <remarks>
    /// This method updates the modification audit metadata of the entity.
    /// Implementations are expected to set the identifier of the actor
    /// responsible for the change and any related modification metadata
    /// (such as timestamps, if applicable).
    /// </remarks>
    void MarkAsEditedBy(Guid actorGuid);
}
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
    /// <see cref="IModifiedEntity.MarkAsEditedBy(Guid)"/>) whenever this member
    /// is modified.
    /// </remarks>
    IModifiedEntity ParentEditedEntity { get; }
}

public interface IModifiedEntityTypes<TModificationType> where TModificationType : Enum
{
    IReadOnlySet<TModificationType> GetModificationTypes();

    TModificationType ModificationTypes { get; }
}
