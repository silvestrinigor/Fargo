using Fargo.Core.Actors;
using Fargo.Core.Common;

namespace Fargo.Application.Common;

/// <summary>
/// Represents an exception that is thrown when a permission denial occurs during application execution.
/// This exception is specifically used to indicate that a particular actor does not have the required 
/// permissions to perform a requested action.
/// </summary>
public class ActorPermissionDeniedFargoApplicationException : FargoApplicationException
{
    /// <summary>
    /// Gets the unique identifier of the actor that was denied permission.
    /// </summary>
    public Guid ActorGuid { get; }

    /// <summary>
    /// Gets the type of the actor that was denied permission.
    /// </summary>
    public ActorType ActorType { get; }

    /// <summary>
    /// Gets the type of action that was denied permission.
    /// </summary>
    public ActionType ActionType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorPermissionDeniedFargoApplicationException"/> class 
    /// with the specified actor details and action type.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor that was denied permission</param>
    /// <param name="actorType">The type of the actor that was denied permission</param>
    /// <param name="actionType">The type of action that was denied permission</param>
    public ActorPermissionDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, ActionType actionType)
        : base($"Action '{actionType}' permission denied for actor '{actorGuid}' of type '{actorType}'.", FargoErrorType.PermissionDenied)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        ActionType = actionType;
    }
}
