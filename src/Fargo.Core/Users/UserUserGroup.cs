using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents the association between a user and a user group.
/// </summary>
/// <remarks>
/// This entity represents a user's membership in a user group.
/// A user may belong to multiple user groups, and a user group may
/// contain multiple users.
/// 
/// The constructor that accepts only a user group GUID is intended for
/// situations where only the foreign key is required, such as creating
/// associations with well-known or otherwise predetermined user group
/// identifiers without loading the corresponding <see cref="UserGroup"/>
/// entity.
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

    /// <summary>
    /// Initializes a new user-group membership using only the user group identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the user group entity is not required
    /// and only its identifier is known, such as when associating a user with a
    /// well-known or otherwise predetermined user group.
    ///
    /// The user group navigation property is intentionally not initialized in
    /// this case. The relationship is represented by <see cref="UserGroupGuid"/>.
    /// </remarks>
    /// <param name="user">The user being associated with the user group.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userGroupGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal UserUserGroup(User user, Guid userGroupGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(user);

        if (userGroupGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "User group GUID cannot be empty.",
                nameof(userGroupGuid));
        }

        User = user;
        UserGuid = user.Guid;

        UserGroupGuid = userGroupGuid;
    }
}
