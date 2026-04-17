namespace Fargo.Domain.Users;

// TODO: validate documentation
/// <summary>
/// Represents a permission assigned to a user group.
/// </summary>
/// <remarks>
/// Each instance defines that a specific <see cref="UserGroup"/> is allowed
/// to perform a particular <see cref="ActionType"/>.
///
/// This entity is part of the <see cref="UserGroup"/> aggregate and represents
/// a single permission entry associated with the group.
///
/// The entity also implements <see cref="IModifiedEntityMember"/>, meaning
/// that any changes to this permission will propagate auditing updates
/// to the parent <see cref="UserGroup"/> entity.
/// </remarks>
public class UserGroupPermission : Entity, IModifiedEntityMember, IPermission
{
    /// <summary>
    /// Gets the unique identifier of the user group that owns this permission.
    /// </summary>
    /// <remarks>
    /// This value mirrors the identifier of the associated <see cref="UserGroup"/>.
    /// It is automatically synchronized when the <see cref="UserGroup"/> property
    /// is assigned.
    /// </remarks>
    public Guid UserGroupGuid
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the user group associated with this permission.
    /// </summary>
    /// <remarks>
    /// When the group is assigned, the <see cref="UserGroupGuid"/> property
    /// is automatically synchronized with the group's identifier.
    ///
    /// This navigation property represents the parent entity in the
    /// aggregate relationship.
    /// </remarks>
    public required UserGroup UserGroup
    {
        get;
        init
        {
            UserGroupGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the action that the user group is allowed to perform.
    /// </summary>
    /// <remarks>
    /// Each permission grants the associated user group the ability to perform
    /// the specified <see cref="ActionType"/>.
    /// </remarks>
    public required ActionType Action
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this permission changes.
    /// </summary>
    /// <remarks>
    /// Since permissions are part of the <see cref="UserGroup"/> aggregate,
    /// modifications to this entity should update the audit metadata of
    /// the parent <see cref="UserGroup"/>.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => UserGroup;
}
