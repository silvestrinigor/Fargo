using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Common;

/// <summary>
/// Represents an exception that is thrown when an actor is denied access to a specific entity.
/// This exception is used to indicate that a particular actor does not have the required 
/// permissions to access a requested entity.
/// </summary>
public class ActorAccessDeniedFargoApplicationException : FargoApplicationException
{
    /// <summary>
    /// Gets the unique identifier of the actor that was denied access to the entity.
    /// </summary>
    public Guid ActorGuid { get; }

    /// <summary>
    /// Gets the type of the actor that was denied access to the entity.
    /// </summary>
    public ActorType ActorType { get; }

    /// <summary>
    /// Gets the unique identifier of the entity that access was denied for.
    /// </summary>
    public Guid EntityGuid { get; }

    /// <summary>
    /// Gets the type of the entity that access was denied for.
    /// </summary>
    public EntityType EntityType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorAccessDeniedFargoApplicationException"/> class 
    /// with the specified actor and entity details.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor that was denied access</param>
    /// <param name="actorType">The type of the actor that was denied access</param>
    /// <param name="entityGuid">The unique identifier of the entity that access was denied for</param>
    /// <param name="entityType">The type of the entity that access was denied for</param>
    public ActorAccessDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, Guid entityGuid, EntityType entityType)
        : base($"Access to entity '{entityGuid}' of type '{entityType}' denied for actor '{actorGuid}' of type '{actorType}'.")
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        EntityGuid = entityGuid;
        EntityType = entityType;
    }
}
