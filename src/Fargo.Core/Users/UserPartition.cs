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
}
