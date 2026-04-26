namespace Fargo.Domain.Users;

/// <summary>
/// Provides domain validation and business rules related to <see cref="User"/> entities.
/// </summary>
/// <remarks>
/// This service encapsulates domain rules involving users, such as uniqueness
/// validation, and self-protection rules.
/// </remarks>
public class UserService(
    IUserRepository userRepository)
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
        CancellationToken cancellationToken = default)
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
    public static void ValidateUserDelete(User user, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(actor);

        if (user.Guid == actor.Guid)
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
    public static void ValidateUserPermissionChange(User user, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(actor);

        if (user.Guid == actor.Guid)
        {
            throw new UserCannotChangeOwnPermissionsFargoDomainException(actor.Guid);
        }

        if (user.Guid == DefaultAdministratorUserGuid)
        {
            throw new ChangeMainAdminUserPermissionsFargoDomainException();
        }
    }
}
