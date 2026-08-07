using Fargo.Core.Shared.Informations;

namespace Fargo.Core.UserGroups;

/// <summary>
/// User group core service.
/// </summary>
public sealed class UserGroupService(IUserGroupRepository userGroupRepository)
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
        var alreadyExistsWithName = await userGroupRepository.ExistsByNameidAsync(nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException(
                $"The userGroup nameid '{nameid}' is already in use.", FargoCoreErrorType.None);
        }
    }

    public async Task ValidateUserGroupDelete(UserGroup userGroup, CancellationToken cancellationToken = default)
    {
        if (userGroup.Guid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid)
        {
            throw new FargoCoreException(
                "The default administrators user group cannot be deleted.", FargoCoreErrorType.None);
        }

        var anyChildUserGroup = await userGroupRepository.HasChildrenUserGroupAsync(userGroup.Guid, cancellationToken);

        if (anyChildUserGroup)
        {
            throw new FargoCoreException(
                $"User group '{userGroup.Guid}' cannot be deleted because it has child user groups.",
                FargoCoreErrorType.None);
        }
    }

    public async Task ValidateParentUserGroupAssignmentAsync(
        UserGroup parentUserGroup, UserGroup memberUserGroup, CancellationToken cancellationToken = default)
    {
        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentUserGroup, memberUserGroup.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            ThrowCircularHierarchy(memberUserGroup.Guid, parentUserGroup.Guid);
        }
    }

    private static void ThrowCircularHierarchy(Guid parent, Guid child) =>
        throw new FargoCoreException(
            $"User group '{child}' cannot be assigned to parent '{parent}' because this would create a circular hierarchy.",
            FargoCoreErrorType.InvalidOperation);

    private async Task<bool> CreatesCircularHierarchyAsync(
        UserGroup candidateParentUserGroup, Guid memberUserGroupGuid, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentUserGroup);

        if (candidateParentUserGroup.Guid == memberUserGroupGuid)
        {
            return true;
        }

        var descendantUserGroupGuids =
            await userGroupRepository.GetDescendantUserGroupGuidsAsync(
                memberUserGroupGuid, false, cancellationToken);

        return descendantUserGroupGuids.Contains(candidateParentUserGroup.Guid);
    }
}
