using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Logics;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;

namespace Fargo.Domain.Services;

/// <summary>
/// Provides domain validation and business rules related to <see cref="User"/> entities.
/// </summary>
/// <remarks>
/// This service encapsulates domain rules involving users, such as uniqueness
/// validation, self-protection rules, and effective permission evaluation.
///
/// Effective permissions may be granted either:
/// <list type="bullet">
/// <item>
/// <description>directly to the user</description>
/// </item>
/// <item>
/// <description>indirectly through one of the user's <see cref="UserGroup"/> memberships</description>
/// </item>
/// </list>
/// </remarks>
public class UserService(
        IUserRepository userRepository,
        IPartitionRepository partitionRepository
        )
{
    /// <summary>
    /// The predefined unique identifier string representing
    /// the default administrator user.
    /// </summary>
    private const string DefaultAdministratorUserGuidString =
        "00000000-0000-0000-0000-000000000004";

    /// <summary>
    /// Gets the predefined unique identifier representing
    /// the default administrator user.
    /// </summary>
    /// <remarks>
    /// This GUID is reserved for the built-in administrator account
    /// created during system initialization.
    /// </remarks>
    public static Guid DefaultAdministratorUserGuid =>
        new(DefaultAdministratorUserGuidString);

    /// <summary>
    /// Validates the rules required to create a new <see cref="User"/>.
    /// </summary>
    /// <param name="user">
    /// The user being created.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserNameidAlreadyExistsDomainException">
    /// Thrown when another user with the same <see cref="User.Nameid"/> already exists.
    /// </exception>
    /// <remarks>
    /// This validation ensures that the <see cref="User.Nameid"/> is unique
    /// within the system.
    /// </remarks>
    public async Task ValidateUserCreate(
            User user,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(user);

        var alreadyExistsWithNameid =
            await userRepository.ExistsByNameid(user.Nameid, cancellationToken);

        if (alreadyExistsWithNameid)
        {
            throw new UserNameidAlreadyExistsDomainException(user.Nameid);
        }
    }

    /// <summary>
    /// Validates the rules required to delete a <see cref="User"/>.
    /// </summary>
    /// <param name="user">
    /// The user being deleted.
    /// </param>
    /// <param name="actor">
    /// The user performing the delete operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> or <paramref name="actor"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserCannotDeleteSelfFargoDomainException">
    /// Thrown when the acting user attempts to delete their own account.
    /// </exception>
    /// <remarks>
    /// This validation ensures that a user cannot delete their own account.
    /// </remarks>
    public static void ValidateUserDelete(User user, User actor)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(actor);

        if (user == actor)
        {
            throw new UserCannotDeleteSelfFargoDomainException(actor.Guid);
        }

        if (user.Guid == DefaultAdministratorUserGuid)
        {
            throw new DeleteMainAdminUserFargoDomainException();
        }
    }

    /// <summary>
    /// Validates the rules required to change a user's permissions.
    /// </summary>
    /// <param name="user">
    /// The user whose permissions are being modified.
    /// </param>
    /// <param name="actor">
    /// The user performing the permission change operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> or <paramref name="actor"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserCannotChangeOwnPermissionsFargoDomainException">
    /// Thrown when the acting user attempts to modify their own permissions.
    /// </exception>
    /// <remarks>
    /// This validation ensures that a user cannot modify their own permissions.
    /// </remarks>
    public static void ValidateUserPermissionChange(User user, User actor)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(actor);

        if (user == actor)
        {
            throw new UserCannotChangeOwnPermissionsFargoDomainException(actor.Guid);
        }

        if (user.Guid == DefaultAdministratorUserGuid)
        {
            throw new ChangeMainAdminUserPermissionsFargoDomainException();
        }
    }

    /// <summary>
    /// Determines whether the specified <paramref name="user"/> has the given permission.
    /// </summary>
    /// <param name="user">
    /// The user whose effective permission set is being evaluated.
    /// </param>
    /// <param name="action">
    /// The action to evaluate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the user has the requested permission,
    /// either directly or through one of their group memberships;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Effective permissions are resolved from both the user's direct permissions
    /// and the permissions inherited from the user's <see cref="UserGroup"/> memberships.
    /// </remarks>
    public static bool HasPermission(User user, ActionType action)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userHasPermission = user.HasPermission(action);

        if (userHasPermission)
        {
            return true;
        }

        var userGroupHasPermission = user.UserGroups.Any(g => g.HasPermission(action));

        return userGroupHasPermission;
    }

    public async Task<UserActor?> GetUserActorByGuid(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuid(userGuid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var parentPartitions = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        parentPartitions.UnionWith(
            user.UserGroups
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.PartitionGuid)
        );

        var partitionAccess = await partitionRepository
            .GetDescendantGuids(parentPartitions, true, cancellationToken);

        return new UserActor(user, partitionAccess);
    }
}
