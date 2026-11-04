using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Actors;

/// <summary>
/// Represents an actor's direct access association with a partition.
/// </summary>
/// <remarks>
/// This entity associates an actor with a partition that the actor is explicitly
/// granted access to. Access to descendant partitions may be inherited from the
/// associated partition according to the partition hierarchy rules.
/// </remarks>
public class ActorPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the associated actor.
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the actor granted access to the partition.
    /// </summary>
    public Actor Actor { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition to which access is granted.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition to which the actor has direct access.
    /// </summary>
    public Partition Partition { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorPartitionAccess"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private ActorPartitionAccess()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Initializes a new actor-partition access association.
    /// </summary>
    /// <param name="actor">The actor being granted access.</param>
    /// <param name="partition">The partition to which access is granted.</param>
    internal ActorPartitionAccess(Actor actor, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(partition);

        Actor = actor;
        ActorGuid = actor.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new actor-partition access association using the specified
    /// partition identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not need to
    /// be loaded and the partition identifier is already known, such as a
    /// well-known or global partition whose identifier is stable.
    /// </remarks>
    /// <param name="actor">The actor being granted access.</param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to which access is granted.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="actor"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    internal ActorPartitionAccess(Actor actor, Guid partitionGuid)
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
