using Fargo.Core.Common;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// User group rules that need to read other groups, so <see cref="UserGroup"/> cannot enforce them
/// alone.
/// </summary>
public sealed class UserGroupService(IUserGroupRepository userGroupRepository)
{
    /// <exception cref="FargoCoreException">Another group already uses the nameid.</exception>
    public async Task ValidateUserGroupNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var alreadyExistsWithName = await userGroupRepository.ExistsByNameidAsync(nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException($"A user group with Nameid '{nameid}' already exists.", FargoErrorType.Conflict);
        }
    }

    /// <exception cref="FargoCoreException">
    /// The group is the administrators group or has child groups.
    /// </exception>
    public async Task ValidateUserGroupCanBeDeletedAsync(UserGroup userGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        userGroup.ValidateCanBeDeleted();

        var anyChildUserGroup = await userGroupRepository.HasChildrenUserGroupAsync(userGroup.Guid, cancellationToken);

        if (anyChildUserGroup)
        {
            throw new FargoCoreException(
                $"User group '{userGroup.Guid}' cannot be deleted because it has child user groups.",
                FargoErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Call before <see cref="UserGroup.SetParentUserGroup(UserGroup)"/>, which cannot detect cycles
    /// on its own. Hold <see cref="IUserGroupRepository.AcquireHierarchyLockAsync"/> so concurrent
    /// re-parents cannot each pass and together create a cycle.
    /// </summary>
    /// <exception cref="FargoCoreException">The assignment would create a cycle.</exception>
    public async Task ValidateParentUserGroupAssignmentHierarchyAsync(
        UserGroup parentUserGroup, UserGroup childUserGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentUserGroup);
        ArgumentNullException.ThrowIfNull(childUserGroup);

        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentUserGroup, childUserGroup.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"User group '{childUserGroup.Guid}' cannot be assigned to parent '{parentUserGroup.Guid}' because this would create a circular hierarchy.",
                FargoErrorType.InvalidOperation);
        }
    }

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
