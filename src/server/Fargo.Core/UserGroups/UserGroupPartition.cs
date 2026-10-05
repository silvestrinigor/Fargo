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

    /// <summary>
    /// Initializes a new user-group partition association using the
    /// specified partition identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not
    /// need to be loaded, such as when associating the user group with a
    /// well-known or global partition whose identifier is already known
    /// and stable.
    ///
    /// Prefer this constructor over the constructor that accepts a
    /// <see cref="Partition"/> when only the partition identifier is required.
    /// This avoids requiring the partition entity to be loaded solely for
    /// creating the association.
    /// </remarks>
    /// <param name="userGroup">
    /// The user group to associate.
    /// </param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to associate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="userGroup"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
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
