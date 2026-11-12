using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// Association between a user group and a partition that scopes the group itself, as opposed to
/// <see cref="UserGroupPartitionAccess"/>, which grants its members access.
/// </summary>
public class UserGroupPartition
{
    public Guid UserGroupGuid { get; private init; }

    public UserGroup UserGroup { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserGroupPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal UserGroupPartition(UserGroup userGroup, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(userGroup);
        ArgumentNullException.ThrowIfNull(partition);

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    // Takes only the identifier so the partition need not be loaded, e.g. for the global partition.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal UserGroupPartition(UserGroup userGroup, Guid partitionGuid)
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
