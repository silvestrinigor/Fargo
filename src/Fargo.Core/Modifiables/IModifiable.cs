namespace Fargo.Core.Modifiables;

/// <summary>
/// Defines the contract for entities that track last modification metadata.
/// </summary>
public interface IModifiable
{
    /// <summary>
    /// Gets the unique identifier of the actor that last modified the entity.
    /// </summary>
    Guid? EditedByActorGuid { get; }
}
