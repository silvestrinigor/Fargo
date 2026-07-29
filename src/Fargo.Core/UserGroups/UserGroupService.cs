using Fargo.Core.Shared;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Provides domain validation and business rules
/// related to <see cref="UserGroup"/> entities.
/// </summary>
public class UserGroupService(IUserGroupRepository userGroupRepository)
{
    /// <summary>
    /// Validates the rules required to create a new <see cref="UserGroup"/>.
    /// </summary>
    public async Task ValidateUserGroupCreate(UserGroup userGroup, CancellationToken cancellationToken = default)
    {
        var alreadyExistsWithName = await userGroupRepository.ExistsByNameid(userGroup.Nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException(
                $"A user group with nameid '{userGroup.Nameid}' already exists.", FargoCoreErrorType.None);
        }
    }

    public async Task ValidateUserGroupNameidChange(UserGroup userGroup, Nameid nameid, CancellationToken cancellationToken = default)
    {
        if (userGroup.Nameid == nameid)
        {
            return;
        }

        var alreadyExistsWithName = await userGroupRepository.ExistsByNameid(nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException(
                $"A user group with nameid '{userGroup.Nameid}' already exists.", FargoCoreErrorType.None);
        }
    }

    /// <summary>
    /// Validates whether a user group can be deleted.
    /// </summary>
    public static void ValidateUserGroupDelete(UserGroup userGroup)
    {
        if (userGroup.Guid == FargoCoreGuids.AdminUserGroupGuid)
        {
            throw new FargoCoreException(
                "The default administrators user group cannot be deleted.", FargoCoreErrorType.None);
        }
    }
}
