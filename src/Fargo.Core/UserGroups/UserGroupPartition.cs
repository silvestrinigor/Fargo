using Fargo.Core.Partitions;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Represents the association between a user group and a partition.
/// </summary>
/// <remarks>
/// This association identifies the partitions with which the user group is
/// associated.
/// </remarks>
public class UserGroupPartition
{
    /// <summary>
    /// Gets the unique identifier of the associated user group.
    /// </summary>
    public Guid UserGroupGuid { get; private init; }

    /// <summary>
    /// Gets the associated user group.
    /// </summary>
    public UserGroup UserGroup { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the associated partition.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the associated partition.
    /// </summary>
    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserGroupPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new user-group partition association.
    /// </summary>
    /// <param name="userGroup">The user group to associate.</param>
    /// <param name="partition">The partition to associate with the user group.</param>
    internal UserGroupPartition(UserGroup userGroup, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(userGroup);
        ArgumentNullException.ThrowIfNull(partition);

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }
}
