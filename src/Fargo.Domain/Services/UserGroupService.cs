using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services;

/// <summary>
/// Provides domain validation and business rules
/// related to <see cref="UserGroup"/> entities.
/// </summary>
public class UserGroupService(
        IUserGroupRepository userGroupRepository
        )
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

    private const string AdministratorsUserGroupGuidString = "00000000-0000-0000-0000-000000000002";

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
            CancellationToken cancellationToken = default
            )
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

    /// <summary>
    /// Validates whether a user group can be deleted by the specified actor.
    /// </summary>
    /// <param name="userGroup">
    /// The user group that is being deleted.
    /// </param>
    /// <param name="actor">
    /// The user attempting to delete the group.
    /// </param>
    /// <exception cref="UserCannotDeleteParentUserGroupFargoDomainException">
    /// Thrown when the actor belongs to the group being deleted.
    /// </exception>
    public static void ValidateUserGroupDelete(
        UserGroup userGroup,
        User actor)
    {
        var actorIsMember = actor.UserGroups
            .Any(x => x.Guid == userGroup.Guid);

        if (actorIsMember)
        {
            throw new UserCannotDeleteParentUserGroupFargoDomainException(userGroup.Guid);
        }
    }
}
