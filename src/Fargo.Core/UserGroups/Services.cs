using Fargo.Core.Actors;
using Fargo.Core.Shared;
using Fargo.Core.Users;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Provides domain validation and business rules
/// related to <see cref="UserGroup"/> entities.
/// </summary>
public class UserGroupService(
    IUserGroupRepository userGroupRepository)
{
    /// <summary>
    /// Gets the predefined unique identifier representing
    /// the default <c>Administrators</c> user group.
    /// </summary>
    /// <remarks>
    /// This GUID is used to identify the built-in administrators group
    /// and should not be reassigned or modified.
    /// </remarks>
    public static Guid AdministratorsUserGroupGuid => new(AdministratorsUserGroupGuidString);

    private const string AdministratorsUserGroupGuidString = "00000000-0000-0000-0000-000000000003";

    /// <summary>
    /// Validates the rules required to create a new <see cref="UserGroup"/>.
    ///
    /// This validation ensures that the <see cref="UserGroup.Nameid"/>
    /// is unique within the system.
    /// </summary>
    /// <param name="userGroup">
    /// The user group being created.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="UserGroupNameidAlreadyExistsDomainException">
    /// Thrown when another user group with the same
    /// <see cref="UserGroup.Nameid"/> already exists.
    /// </exception>
    public async Task ValidateUserGroupCreate(
        UserGroup userGroup,
        CancellationToken cancellationToken = default)
    {
        var alreadyExistsWithName =
            await userGroupRepository.ExistsByNameid(
                    userGroup.Nameid,
                    cancellationToken
                    );

        if (alreadyExistsWithName)
        {
            throw new UserGroupNameidAlreadyExistsDomainException(userGroup.Nameid);
        }
    }

    public async Task ValidateUserGroupNameidChange(
        UserGroup userGroup,
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        if (userGroup.Nameid == nameid)
        {
            return;
        }

        var alreadyExistsWithName =
            await userGroupRepository.ExistsByNameid(
                nameid,
                cancellationToken);

        if (alreadyExistsWithName)
        {
            throw new UserGroupNameidAlreadyExistsDomainException(nameid);
        }
    }

    /// <summary>
    /// Validates whether a user group can be deleted.
    /// </summary>
    /// <param name="userGroup">
    /// The user group that is being deleted.
    /// </param>
    public static void ValidateUserGroupDelete(
        UserGroup userGroup,
        Actor actor,
        IReadOnlyCollection<Guid> actorUserGroupGuids)
    {
        ArgumentNullException.ThrowIfNull(userGroup);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(actorUserGroupGuids);

        userGroup.ValidateCanDelete(actor);

        if (actorUserGroupGuids.Contains(userGroup.Guid))
        {
            throw new UserCannotDeleteParentUserGroupFargoDomainException(userGroup.Guid);
        }

        if (userGroup.Guid == AdministratorsUserGroupGuid)
        {
            throw new DeleteDefaultAdministratorsUserGroupFargoDomainException();
        }
    }
}
