using Fargo.Core.Entities.Actors;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// A permission granted to the members of a user group.
/// </summary>
public class UserGroupPermission
{
    public Guid UserGroupGuid { get; private init; }

    public UserGroup UserGroup { get; private init; }

    public ActionType ActionType { get; private init; }

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    private UserGroupPermission()
#pragma warning restore CS8618
    {
    }

    internal UserGroupPermission(UserGroup userGroup, ActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        UserGroup = userGroup;
        UserGroupGuid = userGroup.Guid;
        ActionType = actionType;
    }
}
