using Fargo.Core.Shared;

namespace Fargo.Core.Users;

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

    public async Task ValidateUserCreateAsync(
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

    public async Task ValidateUserNameidChange(
        User user,
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userWithTheNameid =
            await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (userWithTheNameid is not null && user.Guid != userWithTheNameid.Guid)
        {
            throw new UserNameidAlreadyExistsDomainException(nameid);
        }
    }
}
