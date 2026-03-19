namespace Fargo.Domain.Entities;

/// <summary>
/// Defines the contract for entities that track last modification metadata.
/// </summary>
/// <remarks>
/// Implementations of this interface expose information about the last
/// modification performed on the entity, including the actor responsible
/// for the change.
///
/// The auditing values are typically assigned by the application or
/// infrastructure layer during persistence operations.
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
    /// implementations should typically use
    /// <see cref="Security.SystemActor.Guid"/>.
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
    ///
    /// When the modification is performed by the system, the caller should pass
    /// <see cref="Security.SystemActor.Guid"/>.
    /// </remarks>
    void MarkAsEdited(Guid actorGuid);
}
