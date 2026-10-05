using Fargo.Core.Partitions;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Represents a user group's direct access assignment to a partition.
/// </summary>
/// <remarks>
/// Each instance associates one user group with one partition to which
/// members of the group are granted access. Access to descendant partitions
/// may be inherited through the partition hierarchy.
/// </remarks>
public class UserGroupPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the associated user group.
    /// </summary>
    public Guid UserGroupGuid { get; private init; }

    /// <summary>
    /// Gets the user group granted access to the partition.
    /// </summary>
    public UserGroup UserGroup { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the partition to which access is granted.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the partition to which the user group has direct access.
    /// </summary>
    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserGroupPartitionAccess() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new user-group partition access assignment.
    /// </summary>
    /// <param name="userGroup">
    /// The user group receiving access.
    /// </param>
    /// <param name="partition">
    /// The partition to which access is granted.
    /// </param>
    internal UserGroupPartitionAccess(UserGroup userGroup, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(userGroup);
        ArgumentNullException.ThrowIfNull(partition);

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new user-group partition access assignment using
    /// the identifier of the partition.
    /// </summary>
    /// <param name="userGroup">
    /// The user group receiving access.
    /// </param>
    /// <param name="partitionGuid">
    /// The identifier of the partition to which access is granted.
    /// </param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal UserGroupPartitionAccess(UserGroup userGroup, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;

        PartitionGuid = partitionGuid;
    }
}
