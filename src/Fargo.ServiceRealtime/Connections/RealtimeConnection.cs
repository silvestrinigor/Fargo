using Fargo.Core.Entities.Actors;

namespace Fargo.ServiceRealtime.Connections;

/// <summary>
/// One open hub connection and the partition access of the actor behind it.
/// </summary>
/// <param name="connectionId">The SignalR connection identifier.</param>
/// <param name="actorGuid">The actor that authenticated the connection.</param>
/// <param name="actorType">The kind of actor.</param>
/// <param name="partitionAccessGuids">The partitions the actor can access when the connection opened.</param>
/// <param name="abort">Closes the connection.</param>
public sealed class RealtimeConnection(
    string connectionId,
    Guid actorGuid,
    ActorType actorType,
    IReadOnlySet<Guid> partitionAccessGuids,
    Action abort)
{
    private IReadOnlySet<Guid> partitionAccessGuids = partitionAccessGuids;

    /// <summary>
    /// Gets the SignalR connection identifier.
    /// </summary>
    public string ConnectionId { get; } = connectionId;

    /// <summary>
    /// Gets the actor that authenticated the connection.
    /// </summary>
    public Guid ActorGuid { get; } = actorGuid;

    /// <summary>
    /// Gets the kind of actor.
    /// </summary>
    public ActorType ActorType { get; } = actorType;

    /// <summary>
    /// Gets the partitions the actor can access, as last resolved.
    /// </summary>
    /// <remarks>
    /// Replaced as a whole by <see cref="UpdateAccess"/>, so a reader always sees
    /// one consistent set.
    /// </remarks>
    public IReadOnlySet<Guid> PartitionAccessGuids => Volatile.Read(ref partitionAccessGuids);

    /// <summary>
    /// Replaces the partition access with a newly resolved set.
    /// </summary>
    /// <param name="accessGuids">The partitions the actor can access now.</param>
    public void UpdateAccess(IReadOnlySet<Guid> accessGuids)
    {
        ArgumentNullException.ThrowIfNull(accessGuids);

        Volatile.Write(ref partitionAccessGuids, accessGuids);
    }

    /// <summary>
    /// Determines whether the actor may learn about an entity in the given
    /// partitions.
    /// </summary>
    /// <remarks>
    /// Mirrors the API's read filter: the entity is visible when at least one of
    /// its partitions is accessible. An entity with no partitions is visible to
    /// nobody. This is stricter on purpose than
    /// <see cref="SecurityPrincipal.HasAccess(Core.Entities.Partitions.IPartitionedGuidsReadOnly)"/>,
    /// so a connection never learns about something it could not read.
    /// </remarks>
    /// <param name="partitionGuids">The partitions of the entity.</param>
    public bool CanSee(IReadOnlyCollection<Guid> partitionGuids)
    {
        ArgumentNullException.ThrowIfNull(partitionGuids);

        var access = PartitionAccessGuids;

        foreach (var partitionGuid in partitionGuids)
        {
            if (access.Contains(partitionGuid))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Abort() => abort();
}
