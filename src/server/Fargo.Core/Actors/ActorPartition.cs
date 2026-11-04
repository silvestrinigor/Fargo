using Fargo.Core.Partitions;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents the association between an actor and a partition in which the
/// actor belongs (its partition membership).
/// </summary>
/// <remarks>
/// This association identifies the partitions to which the actor is assigned as
/// a member. Membership defines the actor's partition scope and is distinct from
/// partition access (which governs the partitions the actor is allowed to reach).
/// </remarks>
public class ActorPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated actor.
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the associated actor.
    /// </summary>
    public Actor Actor { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition containing the actor.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition in which the actor belongs.
    /// </summary>
    public Partition Partition { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorPartition"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private ActorPartition()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Initializes a new actor-partition membership.
    /// </summary>
    /// <param name="actor">The actor assigned to the partition.</param>
    /// <param name="partition">The partition to which the actor belongs.</param>
    internal ActorPartition(Actor actor, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(partition);

        Actor = actor;
        ActorGuid = actor.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new actor-partition membership using the specified partition
    /// identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not need to
    /// be loaded, such as when assigning the actor to a well-known or global
    /// partition whose identifier is already known and stable.
    /// </remarks>
    /// <param name="actor">The actor assigned to the partition.</param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to which the actor belongs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="actor"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
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
