using Fargo.Core.Shared;

namespace Fargo.Core.UserGroups;

/// <summary>
/// User group core service.
/// </summary>
public class UserGroupService(IUserGroupRepository userGroupRepository)
{
    /// <summary>
    /// Validates that the specified <paramref name="nameid"/> is available for use
    /// by a user group.
    /// </summary>
    /// <param name="nameid">The name identifier to validate.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when another user group already uses the specified
    /// <paramref name="nameid"/>.
    /// </exception>
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
    /// Validates that the specified <paramref name="userGroup"/> can be deleted.
    /// </summary>
    /// <param name="userGroup">The user group to validate.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown if the user group is the default administrators group.
    /// </exception>
    public static void ValidateUserGroupDelete(UserGroup userGroup)
    {
        if (userGroup.Guid == FargoCoreGuids.AdminUserGroupGuid)
        {
            throw new FargoCoreException(
                "The default administrators user group cannot be deleted.", FargoCoreErrorType.None);
        }
    }
}
