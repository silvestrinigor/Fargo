namespace Fargo.Domain.Entities;

// TODO: validate documentation
/// <summary>
/// Represents the access relationship between a <see cref="User"/> and a <see cref="Partition"/>.
/// </summary>
/// <remarks>
/// A <see cref="UserPartitionAccess"/> defines whether a user is allowed to access a specific
/// partition and optionally whether the user can modify entities within that partition.
///
/// Partitions are used to logically isolate data in the system. Users can only access
/// entities that belong to partitions for which they have an associated
/// <see cref="UserPartitionAccess"/>.
///
/// This entity is a member of the <see cref="User"/> aggregate and implements
/// <see cref="IModifiedEntityMember"/>, meaning that any modification to this
/// entity should update the audit metadata of the parent <see cref="User"/>.
/// </remarks>
public class UserPartitionAccess : Entity, IModifiedEntityMember, IPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the user associated with this access entry.
    /// </summary>
    public Guid UserGuid
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the user that owns this partition access.
    /// </summary>
    /// <remarks>
    /// When the user reference is assigned, <see cref="UserGuid"/> is automatically
    /// synchronized with the user's identifier.
    /// </remarks>
    public required User User
    {
        get;
        set
        {
            UserGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the unique identifier of the partition associated with this access entry.
    /// </summary>
    public Guid PartitionGuid
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the partition to which the user has access.
    /// </summary>
    /// <remarks>
    /// When the partition reference is assigned, <see cref="PartitionGuid"/>
    /// is automatically synchronized with the partition's identifier.
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
    /// Since <see cref="UserPartitionAccess"/> belongs to the <see cref="User"/> aggregate,
    /// the parent audited entity is the associated user.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => User;
}
