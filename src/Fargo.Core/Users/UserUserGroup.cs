using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents the association between a user and a user group.
/// </summary>
/// <remarks>
/// This entity represents a user's membership in a user group.
/// A user may belong to multiple user groups, and a user group may
/// contain multiple users.
/// </remarks>
public class UserUserGroup
{
    /// <summary>
    /// Gets the unique identifier of the associated user.
    /// </summary>
    public Guid UserGuid { get; private init; }

    /// <summary>
    /// Gets the user associated with this membership.
    /// </summary>
    public User User { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the associated user group.
    /// </summary>
    public Guid UserGroupGuid { get; private init; }

    /// <summary>
    /// Gets the user group associated with this membership.
    /// </summary>
    public UserGroup UserGroup { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserUserGroup"/> class.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618
    private UserUserGroup()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Initializes a new user-group membership.
    /// </summary>
    /// <param name="user">
    /// The user being associated with the user group.
    /// </param>
    /// <param name="userGroup">
    /// The user group to which the user belongs.
    /// </param>
    internal UserUserGroup(User user, UserGroup userGroup)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(userGroup);

        User = user;
        UserGuid = user.Guid;

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;
    }
}
