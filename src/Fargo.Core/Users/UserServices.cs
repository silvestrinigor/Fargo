using Fargo.Core.Shared;

namespace Fargo.Core.Users;

/// <summary>
/// Provides domain validation and business rules related to <see cref="User"/> entities.
/// </summary>
public class UserService(
    IUserRepository userRepository)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="nameid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UserNameidAlreadyExistsFargoCoreException"></exception>
    public async Task ValidateUserNameidIsAvailableAsync(
        Nameid nameid, CancellationToken cancellationToken = default)
    {
        var userWithTheNameid =
            await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (userWithTheNameid is not null)
        {
            throw new UserNameidAlreadyExistsFargoCoreException(nameid);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user">The user that will be deleted.</param>
    /// <exception cref="UserAdminDeleteFargoCoreException"></exception>
    public static void ValidateUserCanBeDeleted(User user)
    {
        if (user.Guid == FargoCoreGuids.AdminUserGuid)
        {
            throw new UserAdminDeleteFargoCoreException();
        }
    }
}
