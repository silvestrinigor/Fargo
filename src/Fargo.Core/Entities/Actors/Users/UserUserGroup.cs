using Fargo.Core.Entities.UserGroups;

namespace Fargo.Core.Entities.Actors.Users;

public class UserUserGroup
{
    public Guid UserGuid { get; private init; }

    public User User { get; private init; }

    public Guid UserGroupGuid { get; private init; }

    public UserGroup UserGroup { get; private init; }

    // Required by EF Core.
#pragma warning disable CS8618
    private UserUserGroup()
#pragma warning restore CS8618
    {
    }

    internal UserUserGroup(User user, UserGroup userGroup)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(userGroup);

        User = user;
        UserGuid = user.ActorGuid;

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;
    }

    // Avoids loading the group when only a well-known identifier is needed; the UserGroup
    // navigation is intentionally left unset and the relationship is carried by UserGroupGuid.
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
        UserGuid = user.ActorGuid;

        UserGroupGuid = userGroupGuid;
    }
}
