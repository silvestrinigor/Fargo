namespace Fargo.Application;

/// <summary>
/// Exception thrown when entities outside the actor's partition access are returned.
/// </summary>
public class EntityAccessViolationFargoApplicationException(Guid actorGuid)
    : FargoApplicationException($"Query returned entities outside the partition access of actor '{actorGuid}'.")
{
    /// <summary>
    /// Gets the identifier of the actor whose partition access was violated.
    /// </summary>
    public Guid ActorGuid { get; } = actorGuid;
}