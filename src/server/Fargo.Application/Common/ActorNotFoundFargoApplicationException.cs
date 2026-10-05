using Fargo.Core.Actors;
using Fargo.Core.Common;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common;

/// <summary>
/// Represents an exception that is thrown when a requested actor cannot be found in the system.
/// This exception inherits from <see cref="FargoApplicationException"/> and is specifically 
/// categorized as an entity not found error.
/// </summary>
public class ActorNotFoundFargoApplicationException : FargoApplicationException
{
    /// <summary>
    /// Gets the unique identifier of the actor that was not found.
    /// </summary>
    public Guid ActorGuid { get; }

    /// <summary>
    /// Gets the type of the actor that was not found.
    /// </summary>
    public ActorType ActorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorNotFoundFargoApplicationException"/> class
    /// with the specified actor GUID and actor type.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor that was not found.</param>
    /// <param name="actorType">The type of the actor that was not found.</param>
    public ActorNotFoundFargoApplicationException(Guid actorGuid, ActorType actorType)
        : base($"Actor '{actorGuid}' of type '{actorType}' was not found.", FargoErrorType.EntityNotFound)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
    }

    /// <summary>
    /// Throws an <see cref="ActorNotFoundFargoApplicationException"/> if the specified actor is null.
    /// This method provides a convenient way to check for null actors and throw an appropriate exception
    /// when they are encountered.
    /// </summary>
    /// <param name="actor">The actor to check for null.</param>
    /// <param name="actorGuid">The unique identifier of the actor being checked.</param>
    /// <param name="actorType">The type of the actor being checked.</param>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the <paramref name="actor"/> parameter is null.
    /// </exception>
    public static void ThrowIfNull([NotNull] Actor? actor, Guid actorGuid, ActorType actorType)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoApplicationException(actorGuid, actorType);
        }
    }
}
