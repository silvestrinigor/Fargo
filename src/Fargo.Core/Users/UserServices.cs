using Fargo.Core.Shared;

namespace Fargo.Core.Users;


/// <summary>
/// The user core service.
/// </summary>
public class UserService(IUserRepository userRepository)
{
    /// <summary>
    /// Validates that the specified <paramref name="nameid"/> is not already
    /// assigned to another user.
    /// </summary>
    /// <param name="nameid">The user identifier to validate.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when another user already exists with the specified <paramref name="nameid"/>.
    /// </exception>
    public async Task ValidateUserNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var userWithTheNameid = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (userWithTheNameid is not null)
        {
            throw new FargoCoreException($"A user with Nameid '{nameid}' already exists.", FargoCoreErrorType.None);
        }
    }

    /// <summary>
    /// Validates that the specified <paramref name="user"/> can be deleted.
    /// </summary>
    /// <param name="user">The user to validate.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown if the user is the main administrator.
    /// </exception>
    public static void ValidateUserCanBeDeleted(User user)
    {
        if (user.Guid == FargoCoreGuids.AdminUserGuid)
        {
            throw new FargoCoreException("The main administrator user cannot be deleted.", FargoCoreErrorType.None);
        }
    }
}
