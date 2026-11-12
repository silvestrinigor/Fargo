using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// A direct grant of access to a partition for the members of a user group; access to descendant
/// partitions is inherited through the hierarchy.
/// </summary>
public class UserGroupPartitionAccess
{
    public Guid UserGroupGuid { get; private init; }

    public UserGroup UserGroup { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserGroupPartitionAccess() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal UserGroupPartitionAccess(UserGroup userGroup, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(userGroup);
        ArgumentNullException.ThrowIfNull(partition);

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

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
