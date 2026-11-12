using Fargo.Core.Common;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Actors.Users;

/// <summary>
/// The human-user data of a user <see cref="Actor"/>.
/// </summary>
/// <remarks>
/// Owned satellite of the actor; shared authorization state lives on the <see cref="Actors.Actor"/>.
/// The built-in administrator is restricted to the global partition and is a member of the
/// administrators user group; these associations cannot be revoked.
/// </remarks>
public class User
{
    public Guid ActorGuid { get; private init; }

    public Actor Actor { get; private init; } = null!;

    public bool IsAdmin => ActorGuid == FargoCoreWellKnowGuids.AdminUserGuid;

    public FirstName? FirstName { get; set; } = null;

    public LastName? LastName { get; set; } = null;

    public UserAuthentication Authentication { get; private init; }

    public IReadOnlyCollection<UserUserGroup> UserGroupMemberships => userGroupMemberships;

    private readonly List<UserUserGroup> userGroupMemberships = [];

    // Required by EF Core.
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

    internal static User CreateUser(Actor actor)
        => new(actor);

    internal static User CreateAdministratorUser(Actor actor)
    {
        var user = new User(actor);

        user.AddUserGroup(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid);

        return user;
    }

    /// <summary>
    /// Does nothing if the user already belongs to the group.
    /// </summary>
    public void AddUserGroup(UserGroup userGroup)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        if (userGroupMemberships.Any(x => x.UserGroupGuid == userGroup.Guid))
        {
            return;
        }

        userGroupMemberships.Add(new UserUserGroup(this, userGroup));
    }

    // Avoids loading the group when only a well-known identifier is needed.
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

    /// <exception cref="FargoCoreException">
    /// The user is the administrator and the group is the administrators group.
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
