namespace Fargo.Application.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a partitioned entity
/// without having permission to any of its associated partitions.
/// </summary>
public sealed class PartitionedEntityAccessDeniedFargoApplicationException(
        Guid entityGuid,
        Guid userGuid
        )
    : FargoApplicationException(
        $"User '{userGuid}' does not have access to entity '{entityGuid}'."
        )
{
    /// <summary>
    /// Gets the unique identifier of the entity that access was denied for.
    /// </summary>
    public Guid EntityGuid { get; } = entityGuid;

    /// <summary>
    /// Gets the unique identifier of the user who attempted the operation.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
