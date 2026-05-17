using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

#region Entity

/// <summary>
/// Represents a user in the system.
/// </summary>
/// <remarks>
/// A user contains authentication credentials, direct permissions,
/// partition access, and group memberships that may grant additional
/// permissions and access.
///
/// Authorization for a user is determined by the combination of:
/// - Direct permissions and partition access
/// - Permissions and partition access inherited from user groups
/// </remarks>
public class User : Entity, IModifiedEntity, IModifiedEntityTypes<UserModifiedType>, IPartitionedEntity, IPartitionUser, IPartitioned, IPermissionUser, IActivable
{
    private User()
    {
    }

    public User(Nameid nameid, PasswordHash passwordHash)
    {
        Nameid = nameid;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Gets or sets the unique NAMEID (username) of the user.
    /// This value identifies the user in the system and must satisfy
    /// the validation rules defined by <see cref="Nameid"/>.
    /// </summary>
    public Nameid Nameid
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the user's first name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="FirstName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the first name
    /// has not been specified.
    /// </remarks>
    public FirstName? FirstName
    {
        get;
        private set;
    } = null;

    /// <summary>
    /// Gets or sets the user's last name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="LastName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the last name
    /// has not been specified.
    /// </remarks>
    public LastName? LastName
    {
        get;
        private set;
    } = null;

    /// <summary>
    /// Gets or sets the textual description associated with the user.
    /// If not specified, the description defaults to <see cref="Description.Empty"/>.
    /// </summary>
    public Description Description
    {
        get;
        private set;
    } = Description.Empty;

    public void ChangeNameid(Nameid nameid)
    {
        if (Nameid == nameid)
        {
            return;
        }

        Nameid = nameid;
    }

    public void ChangeFirstName(FirstName? firstName)
    {
        if (FirstName == firstName)
        {
            return;
        }

        FirstName = firstName;
    }

    public void ChangeLastName(LastName? lastName)
    {
        if (LastName == lastName)
        {
            return;
        }

        LastName = lastName;
    }

    public void ChangeDescription(Description description)
    {
        if (Description == description)
        {
            return;
        }

        Description = description;
    }

    #region Active

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate() => IsActive = false;

    #endregion Active

    #region Password

    /// <summary>
    /// Gets or sets the hashed password of the user.
    ///
    /// The raw password is never stored. Instead, a secure hash
    /// is persisted using the application's password hashing strategy.
    /// </summary>
    public PasswordHash PasswordHash
    {
        get;
        private set;
    }

    public TimeSpan? DefaultPasswordExpirationPeriod { get; private set; } = null;

    public DateTimeOffset? RequirePasswordChangeAt { get; private set; } = null;

    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

    /// <summary>
    /// Resets the password expiration date based on the user's
    /// <see cref="DefaultPasswordExpirationPeriod"/>.
    ///
    /// The new expiration date is calculated by adding the configured
    /// default expiration interval to the current UTC time.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately.
    /// </summary>
    /// <remarks>
    /// This method is typically used after the user successfully changes
    /// their own password.
    /// </remarks>
    public void ResetPasswordExpiration()
        => RequirePasswordChangeAt = DateTimeOffset.UtcNow + DefaultPasswordExpirationPeriod;

    /// <summary>
    /// Sets the password expiration requirement to a future date based on the specified number of days.
    /// </summary>
    /// <param name="days">
    /// The number of days from the current UTC time after which the user must change their password.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="days"/> is less than zero.
    /// </exception>
    public void RequirePasswordChangeInDays(int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);

        RequirePasswordChangeAt = DateTimeOffset.UtcNow.AddDays(days);
    }

    /// <summary>
    /// Marks the user's password as requiring an immediate change.
    /// </summary>
    /// <remarks>
    /// After calling this method, <see cref="IsPasswordChangeRequired"/> will return <c>true</c>
    /// until the password is updated and a new expiration date is set.
    /// </remarks>
    public void MarkPasswordChangeAsRequired()
    {
        RequirePasswordChangeAt = DateTimeOffset.UtcNow;
    }

    public void ChangePasswordHash(PasswordHash passwordHash)
    {
        if (PasswordHash == passwordHash)
        {
            return;
        }

        PasswordHash = passwordHash;
    }

    public void SetDefaultPasswordExpirationPeriod(TimeSpan? expirationPeriod)
    {
        if (DefaultPasswordExpirationPeriod == expirationPeriod)
        {
            return;
        }

        DefaultPasswordExpirationPeriod = expirationPeriod;
    }

    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    #endregion Password

    #region Permission

    /// <summary>
    /// Gets the read-only collection of permissions assigned directly to the user.
    ///
    /// Each permission represents an allowed <see cref="ActionType"/>
    /// that the user can perform without considering group memberships.
    /// </summary>
    public IReadOnlyCollection<UserPermission> Permissions
    {
        get => permissions;
        init => permissions = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPermission> IPermissionUser.Permissions => Permissions;

    private readonly List<UserPermission> permissions = [];

    /// <summary>
    /// Adds a permission to the user if it does not already exist.
    /// </summary>
    /// <param name="action">The action type to allow.</param>
    public void AddPermission(ActionType action)
    {
        if (permissions.Any(x => x.Action == action))
        {
            return;
        }

        var userPermission = new UserPermission
        {
            Action = action,
            User = this
        };

        permissions.Add(userPermission);
    }

    /// <summary>
    /// Removes a permission from the user if it exists.
    /// </summary>
    /// <param name="action">The action type to remove.</param>
    public void RemovePermission(ActionType action)
    {
        var userPermission = permissions.SingleOrDefault(x => x.Action == action);

        if (userPermission == null)
        {
            return;
        }

        permissions.Remove(userPermission);
    }

    #endregion Permission

    /// <summary>
    /// Gets the collection of groups the user belongs to.
    /// </summary>
    /// <remarks>
    /// User groups provide additional permissions and partition access
    /// that are inherited by the user.
    ///
    /// Effective authorization for a user is typically the combination of:
    /// - Direct permissions and partition access
    /// - Permissions and partition access inherited from groups
    /// </remarks>
    public UserGroupCollection UserGroups { get; init; } = [];

    #region Partition

    /// <summary>
    /// Gets the read-only collection of partitions the user has access to.
    /// </summary>
    /// <remarks>
    /// Partitions define logical boundaries in the system.
    /// A user can access entities that have no partition (public), or that
    /// belong to at least one partition to which the user has been granted access.
    /// </remarks>
    public IReadOnlyCollection<UserPartitionAccess> PartitionAccesses
    {
        get => partitionAccesses;
        init => partitionAccesses = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionAccess> IPartitionUser.PartitionAccesses => PartitionAccesses;

    private readonly List<UserPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Grants access to the specified partition for the user.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        var partitionAccess = new UserPartitionAccess
        {
            User = this,
            Partition = partition
        };

        partitionAccesses.Add(partitionAccess);
    }

    /// <summary>
    /// Removes access to the specified partition from the user.
    /// </summary>
    /// <param name="partitionGuid">The partition identifier.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        var userPartition =
            partitionAccesses.SingleOrDefault(x => x.PartitionGuid == partitionGuid);

        if (userPartition == null)
        {
            return;
        }

        partitionAccesses.Remove(userPartition);
    }

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the user entity itself,
    /// not the partitions the user has access to.
    ///
    /// This is used for partition-based access control on the user entity,
    /// meaning a user can only access this user record if they have access
    /// to at least one of these partitions.
    ///
    /// To determine which partitions the user can access, see
    /// <see cref="PartitionAccesses"/> and <see cref="UserGroups"/>.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. Partitions.Select(p => p.Guid)];

    public void AddPartition(Partition partition)
    {
        Partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        Partitions.Remove(partition);
    }

    public void AddUserGroup(UserGroup userGroup)
    {
        UserGroups.Add(userGroup);
    }

    public void RemoveUserGroup(UserGroup userGroup)
    {
        UserGroups.Remove(userGroup);
    }

    #endregion Partition

    #region Modified

    public Guid? EditedByGuid { get; private set; }

    public void MarkAsEditedBy(Guid actorGuid)
    {
        EditedByGuid = actorGuid;
    }

    public UserModifiedType ModificationTypes { get; private set; }

    public IReadOnlySet<UserModifiedType> GetModificationTypes()
    {
        HashSet<UserModifiedType> result = [];

        foreach (UserModifiedType value in Enum.GetValues<UserModifiedType>())
        {
            if (value == UserModifiedType.None)
            {
                continue;
            }

            if ((ModificationTypes & value) != 0)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public bool IsEditStarted { get; private set; }

    public void MarkModificationType(UserModifiedType modificationType)
    {
        if (!IsEditStarted)
        {
            ModificationTypes = UserModifiedType.None;
            IsEditStarted = true;
        }

        ModificationTypes |= modificationType;
    }

    #endregion Modified
}

#endregion Entity

#region Permissions

/// <summary>
/// Represents an object that grants a specific permission action.
/// </summary>
/// <remarks>
/// This abstraction allows different permission sources, such as direct user
/// permissions or group permissions, to be evaluated uniformly.
/// </remarks>
public interface IPermission
{
    /// <summary>
    /// Gets the action granted by the permission.
    /// </summary>
    ActionType Action { get; }
}
/// <summary>
/// Represents an object that exposes a read-only collection of permissions.
/// </summary>
/// <remarks>
/// Implementations typically include domain entities such as users or user groups
/// that participate in authorization checks.
/// </remarks>
public interface IPermissionUser
{
    /// <summary>
    /// Gets the read-only collection of permissions associated with the object.
    /// </summary>
    IReadOnlyCollection<IPermission> Permissions { get; }
}
/// <summary>
/// Represents a permission granted to a user or role within the system.
/// </summary>
/// <remarks>
/// A permission defines an action that can be performed in the system.
/// This value object is typically used to represent authorization rules
/// associated with users, groups, or other security-related entities.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the permission.
/// </param>
/// <param name="Action">
/// The action that the permission allows to be performed.
/// </param>
public sealed record Permission(
    Guid Guid,
    ActionType Action
);
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
    public Guid UserGuid { get; private set; }

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
    public required ActionType Action { get; init; }

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

#endregion Permissions

#region Partition Access

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
    public Guid UserGuid { get; private set; }

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
    public Guid PartitionGuid { get; private set; }

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

#endregion Partition Access
