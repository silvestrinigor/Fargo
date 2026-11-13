using Fargo.Core.Entities.Actors;

namespace Fargo.Realtime.Connections;

/// <summary>
/// One open hub connection and the partition access of the actor behind it.
/// </summary>
public sealed class RealtimeConnection(
    string connectionId,
    Guid actorGuid,
    ActorType actorType,
    IReadOnlySet<Guid> partitionAccessGuids,
    Action abort)
{
    private IReadOnlySet<Guid> partitionAccessGuids = partitionAccessGuids;

    public string ConnectionId { get; } = connectionId;

    public Guid ActorGuid { get; } = actorGuid;

    public ActorType ActorType { get; } = actorType;

    /// <summary>
    /// The partitions the actor can access, as last resolved. Replaced as a whole by <see cref="UpdateAccess"/>,
    /// so a reader always sees one consistent set.
    /// </summary>
    public IReadOnlySet<Guid> PartitionAccessGuids => Volatile.Read(ref partitionAccessGuids);

    public void UpdateAccess(IReadOnlySet<Guid> accessGuids)
    {
        ArgumentNullException.ThrowIfNull(accessGuids);

        Volatile.Write(ref partitionAccessGuids, accessGuids);
    }

    /// <summary>
    /// Whether the actor can access at least one of the given partitions; an entity with no partitions is
    /// visible to nobody.
    /// </summary>
    /// <remarks>
    /// Mirrors the API's read filter. Stricter on purpose than
    /// <see cref="SecurityPrincipal.HasAccess(Core.Entities.Partitions.IPartitionedGuidsReadOnly)"/>,
    /// which accepts an entity with no partitions, so a connection never learns about something it could
    /// not read.
    /// </remarks>
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

    public void Abort() => abort();
}
