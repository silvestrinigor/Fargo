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
    /// <exception cref="UserNameidAlreadyExistsDomainException"></exception>
    public async Task ValidateUserNameidIsAvailableAsync(
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        var userWithTheNameid =
            await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (userWithTheNameid is not null)
        {
            throw new UserNameidAlreadyExistsDomainException(nameid);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <exception cref="DeleteMainAdminUserFargoException"></exception>
    public static void ValidateUserDelete(User user)
    {
        if (user.Guid == FargoConstantGuids.AdminUserGuid)
        {
            throw new DeleteMainAdminUserFargoException();
        }
    }
}
