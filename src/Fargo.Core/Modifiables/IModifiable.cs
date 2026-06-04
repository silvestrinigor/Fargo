namespace Fargo.Core.Modifiables;

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
public interface IModifiable
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
