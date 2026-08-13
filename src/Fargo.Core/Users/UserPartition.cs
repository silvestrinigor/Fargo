using Fargo.Core.Partitions;

namespace Fargo.Core.Users;

/// <summary>
/// Represents the association between a user and a partition in which the user belongs.
/// </summary>
/// <remarks>
/// This association identifies the partitions to which the user is assigned as
/// a member.
/// </remarks>
public class UserPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated user.
    /// </summary>
    public Guid UserGuid { get; private init; }

    /// <summary>
    /// Gets the associated user.
    /// </summary>
    public User User { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition containing the user.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition in which the user belongs.
    /// </summary>
    public Partition Partition { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPartition"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserPartition()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    /// <summary>
    /// Initializes a new user-partition membership.
    /// </summary>
    /// <param name="user">The user assigned to the partition.</param>
    /// <param name="partition">The partition to which the user belongs.</param>
    internal UserPartition(User user, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(partition);

        User = user;
        UserGuid = user.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new user-partition membership using the specified
    /// partition identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not
    /// need to be loaded, such as when assigning the user to a well-known
    /// or global partition whose identifier is already known and stable.
    ///
    /// Prefer this constructor over the constructor that accepts a
    /// <see cref="Partition"/> when only the partition identifier is required.
    /// This avoids requiring the partition entity to be loaded solely for
    /// creating the association.
    /// </remarks>
    /// <param name="user">
    /// The user assigned to the partition.
    /// </param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to which the user belongs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal UserPartition(User user, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(user);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        User = user;
        UserGuid = user.Guid;

        PartitionGuid = partitionGuid;
    }
}
