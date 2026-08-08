using Fargo.Core.Commom;
using Fargo.Core.Shared.Informations;

namespace Fargo.Core.UserGroups;

/// <summary>
/// User group core service.
/// </summary>
public sealed class UserGroupService(IUserGroupRepository userGroupRepository)
{
    /// <summary>
    /// Validates that the specified <see cref="Nameid"/> is available for use by a
    /// user group.
    /// </summary>
    /// <param name="nameid">The name identifier to validate.</param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when another user group already uses the specified
    /// <see cref="Nameid"/>.
    /// </exception>
    public async Task ValidateUserGroupNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var alreadyExistsWithName = await userGroupRepository.ExistsByNameidAsync(nameid, cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new FargoCoreException($"A user group with Nameid '{nameid}' already exists.", FargoCoreErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Validates that the specified user group can be deleted.
    /// </summary>
    /// <param name="userGroup">
    /// The user group to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to delete the built-in administrators user group
    /// or when the user group has one or more child user groups.
    /// </exception>
    public async Task ValidateUserGroupCanBeDeletedAsync(UserGroup userGroup, CancellationToken cancellationToken = default)
    {
        if (userGroup.IsAdministrators)
        {
            throw new FargoCoreException(
                $"The administrators user group '{userGroup.Guid}' cannot be deleted.", FargoCoreErrorType.InvalidOperation);
        }

        var anyChildUserGroup = await userGroupRepository.HasChildrenUserGroupAsync(userGroup.Guid, cancellationToken);

        if (anyChildUserGroup)
        {
            throw new FargoCoreException(
                $"User group '{userGroup.Guid}' cannot be deleted because it has child user groups.",
                FargoCoreErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Validates that assigning the specified parent user group to the specified
    /// child user group would result in a valid hierarchy.
    /// </summary>
    /// <param name="parentUserGroup">
    /// The user group that will become the parent.
    /// </param>
    /// <param name="childUserGroup">
    /// The user group that will become the child.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentUserGroup"/> or
    /// <paramref name="childUserGroup"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the assignment would create a circular user group hierarchy.
    /// </exception>
    /// <remarks>
    /// This method should be called before
    /// <see cref="UserGroup.SetParentUserGroup(UserGroup)"/> because validating
    /// the complete hierarchy requires access to other user groups through the
    /// repository.
    /// </remarks>
    public async Task ValidateParentUserGroupAssignmentHierarchyAsync(
        UserGroup parentUserGroup, UserGroup childUserGroup, CancellationToken cancellationToken = default)
    {
        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentUserGroup, childUserGroup.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"User group '{childUserGroup.Guid}' cannot be assigned to parent '{parentUserGroup.Guid}' because this would create a circular hierarchy.",
                FargoCoreErrorType.InvalidOperation);
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
