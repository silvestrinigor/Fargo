using Fargo.Core.Shared;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Provides domain validation and business rules
/// related to <see cref="UserGroup"/> entities.
/// </summary>
public class UserGroupService(IUserGroupRepository userGroupRepository)
{
    public async Task ValidateUserGroupNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var alreadyExistsWithName = await userGroupRepository.ExistsByNameid(nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException(
                $"The userGroup nameid '{nameid}' is already in use.", FargoCoreErrorType.None);
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
