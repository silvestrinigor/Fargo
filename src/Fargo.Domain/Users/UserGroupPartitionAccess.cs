using Fargo.Domain.Partitions;

namespace Fargo.Domain.Users;

/// <summary>
/// Represents the access relationship between a <see cref="UserGroup"/> and a <see cref="Partition"/>.
/// </summary>
/// <remarks>
/// A <see cref="UserGroupPartitionAccess"/> defines whether a user group is allowed
/// to access a specific partition.
///
/// Partitions are used to logically isolate data in the system. Users that belong
/// to a group inherit access to the partitions associated with that group through
/// <see cref="UserGroupPartitionAccess"/>.
///
/// This entity is a member of the <see cref="UserGroup"/> aggregate and implements
/// <see cref="IModifiedEntityMember"/>, meaning that any modification to this
/// entity should update the audit metadata of the parent <see cref="UserGroup"/>.
/// </remarks>
public class UserGroupPartitionAccess : Entity, IModifiedEntityMember, IPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the user group associated with this access entry.
    /// </summary>
    public Guid UserGroupGuid { get; private set; }

    /// <summary>
    /// Gets or sets the user group that owns this partition access.
    /// </summary>
    /// <remarks>
    /// When the user group reference is assigned, <see cref="UserGroupGuid"/> is
    /// automatically synchronized with the group identifier.
    /// </remarks>
    public required UserGroup UserGroup
    {
        get;
        set
        {
            UserGroupGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the unique identifier of the partition associated with this access entry.
    /// </summary>
    public Guid PartitionGuid { get; private set; }

    /// <summary>
    /// Gets or sets the partition to which the user group has access.
    /// </summary>
    /// <remarks>
    /// When the partition reference is assigned, <see cref="PartitionGuid"/>
    /// is automatically synchronized with the partition identifier.
    /// </remarks>
    public required Partition Partition
    {
        get;
        set
        {
            PartitionGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// Since <see cref="UserGroupPartitionAccess"/> belongs to the
    /// <see cref="UserGroup"/> aggregate, the parent audited entity is the
    /// associated user group.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => UserGroup;
}
