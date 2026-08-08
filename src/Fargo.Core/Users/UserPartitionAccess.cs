using Fargo.Core.Partitions;

namespace Fargo.Core.Users;

/// <summary>
/// Represents a user's direct access association with a partition.
/// </summary>
/// <remarks>
/// This entity associates a user with a partition that the user is explicitly
/// granted access to. Access to descendant partitions may be inherited from
/// the associated partition according to the partition hierarchy rules.
/// </remarks>
public class UserPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the associated user.
    /// </summary>
    public Guid UserGuid { get; private init; }

    /// <summary>
    /// Gets the user granted access to the partition.
    /// </summary>
    public User User { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition to which access is granted.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition to which the user has direct access.
    /// </summary>
    public Partition Partition { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPartitionAccess"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618
    private UserPartitionAccess()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Initializes a new user-partition access association.
    /// </summary>
    /// <param name="user">
    /// The user being granted access.
    /// </param>
    /// <param name="partition">
    /// The partition to which access is granted.
    /// </param>
    internal UserPartitionAccess(User user, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(partition);

        User = user;
        UserGuid = user.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }
}
