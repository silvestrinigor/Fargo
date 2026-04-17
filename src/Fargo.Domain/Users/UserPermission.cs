namespace Fargo.Domain.Users;

// TODO: validate documentation
/// <summary>
/// Represents a permission assigned to a user.
/// </summary>
/// <remarks>
/// Each instance defines that a specific <see cref="User"/> is allowed
/// to perform a particular <see cref="ActionType"/>.
///
/// This entity is part of the <see cref="User"/> aggregate and represents
/// a single permission entry associated with the user.
///
/// The entity also implements <see cref="IModifiedEntityMember"/>, meaning
/// that any changes to this permission will propagate auditing updates
/// to the parent <see cref="User"/> entity.
/// </remarks>
public class UserPermission : Entity, IModifiedEntityMember, IPermission
{
    /// <summary>
    /// Gets the unique identifier of the user that owns this permission.
    /// </summary>
    /// <remarks>
    /// This value mirrors the identifier of the associated <see cref="User"/>.
    /// It is automatically synchronized when the <see cref="User"/> property
    /// is assigned.
    /// </remarks>
    public Guid UserGuid
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the user associated with this permission.
    /// </summary>
    /// <remarks>
    /// When the user is assigned, the <see cref="UserGuid"/> property
    /// is automatically synchronized with the user's identifier.
    ///
    /// This navigation property represents the parent entity in the
    /// aggregate relationship.
    /// </remarks>
    public required User User
    {
        get;
        init
        {
            UserGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the action that the user is allowed to perform.
    /// </summary>
    /// <remarks>
    /// Each permission grants the associated user the ability to perform
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
    /// Since permissions are part of the <see cref="User"/> aggregate,
    /// modifications to this entity should update the audit metadata of
    /// the parent <see cref="User"/>.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => User;
}
