using Fargo.Core.Entities.Actors;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Core.Entities.Actors.Users;

/// <summary>
/// Represents the user-specific data of a user <see cref="Actor"/>.
/// </summary>
/// <remarks>
/// A user is an owned satellite of an <see cref="Actors.Actor"/> (keyed by the
/// owning actor's identifier), in the same way an
/// <see cref="Articles.ArticleVariation"/> is an owned satellite of an
/// <see cref="Articles.Article"/>. The authorization state shared by every actor
/// (permissions, partition membership and partition accesses) lives on the owning
/// <see cref="Actors.Actor"/>; this type carries only the fields unique to a human
/// user.
///
/// The built-in administrator user is restricted to the global partition and is a
/// member of the built-in administrators user group. These associations cannot be
/// revoked.
/// </remarks>
public class User
{
    /// <summary>
    /// Gets the unique identifier of the owning actor (and therefore of the user).
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the actor that owns this user data.
    /// </summary>
    public Actor Actor { get; private init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user is the built-in administrator.
    /// </summary>
    public bool IsAdmin => ActorGuid == FargoCoreWellKnowGuids.AdminUserGuid;

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public FirstName? FirstName { get; set; } = null;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public LastName? LastName { get; set; } = null;

    /// <summary>
    /// Gets the authentication information associated with the user.
    /// </summary>
    public UserAuthentication Authentication { get; private init; }

    /// <summary>
    /// Gets the user-group memberships associated with the user.
    /// </summary>
    public IReadOnlyCollection<UserUserGroup> UserGroupMemberships => userGroupMemberships;

    private readonly List<UserUserGroup> userGroupMemberships = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <remarks>Required by Entity Framework.</remarks>
    private User()
    {
        Authentication = new UserAuthentication(this);
    }

    private User(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;
        ActorGuid = actor.Guid;
        Authentication = new UserAuthentication(this);
    }

    /// <summary>
    /// Creates a new user for the specified owning actor.
    /// </summary>
    /// <param name="actor">The owning actor.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    internal static User CreateUser(Actor actor)
        => new(actor);

    /// <summary>
    /// Creates the built-in administrator user for the specified owning actor.
    /// </summary>
    /// <remarks>
    /// The administrator is added to the built-in administrators user group. This
    /// association is mandatory and cannot be revoked.
    /// </remarks>
    /// <param name="actor">The owning actor.</param>
    /// <returns>The built-in administrator <see cref="User"/>.</returns>
    internal static User CreateAdministratorUser(Actor actor)
    {
        var user = new User(actor);

        user.AddUserGroup(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid);

        return user;
    }

    /// <summary>
    /// Adds the user to the specified user group.
    /// If the user already belongs to the group, no action is taken.
    /// </summary>
    /// <param name="userGroup">The user group to add.</param>
    public void AddUserGroup(UserGroup userGroup)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        if (userGroupMemberships.Any(x => x.UserGroupGuid == userGroup.Guid))
        {
            return;
        }

        userGroupMemberships.Add(new UserUserGroup(this, userGroup));
    }

    /// <summary>
    /// Adds the user to the specified user group using its identifier.
    /// </summary>
    /// <remarks>
    /// This overload is intended for internal use when the user group entity does
    /// not need to be loaded and only its identifier is known, such as when
    /// assigning membership to a well-known user group.
    /// </remarks>
    /// <param name="userGroupGuid">The identifier of the user group to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userGroupGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    internal void AddUserGroup(Guid userGroupGuid)
    {
        if (userGroupGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "User group GUID cannot be empty.",
                nameof(userGroupGuid));
        }

        if (userGroupMemberships.Any(x => x.UserGroupGuid == userGroupGuid))
        {
            return;
        }

        userGroupMemberships.Add(new UserUserGroup(this, userGroupGuid));
    }

    /// <summary>
    /// Removes the user from the specified user group.
    /// </summary>
    /// <param name="userGroupGuid">The identifier of the user group to remove.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove an administrator from the administrators
    /// user group.
    /// </exception>
    public void RemoveUserGroup(Guid userGroupGuid)
    {
        if (IsAdmin && userGroupGuid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid)
        {
            throw new FargoCoreException(
                $"Cannot remove the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' from the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        userGroupMemberships.RemoveAll(g => g.UserGroupGuid == userGroupGuid);
    }
}
