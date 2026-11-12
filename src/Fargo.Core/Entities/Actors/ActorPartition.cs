using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Actors;

/// <summary>
/// An actor's membership of a partition. Distinct from <see cref="ActorPartitionAccess"/>, which
/// governs the partitions the actor is allowed to reach.
/// </summary>
public class ActorPartition
{
    public Guid ActorGuid { get; private init; }

    public Actor Actor { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private ActorPartition()
#pragma warning restore CS8618
    {
    }

    internal ActorPartition(Actor actor, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(partition);

        Actor = actor;
        ActorGuid = actor.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    // Avoids loading the partition when only a well-known identifier (e.g. global) is needed.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    internal ActorPartition(Actor actor, Guid partitionGuid)
#pragma warning restore CS8618
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        Actor = actor;
        ActorGuid = actor.Guid;

        PartitionGuid = partitionGuid;
    }
}
