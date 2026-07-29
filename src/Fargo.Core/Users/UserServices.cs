using Fargo.Core.Shared;

namespace Fargo.Core.Users;

public class UserService(IUserRepository userRepository)
{
    public async Task ValidateUserNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var userWithTheNameid = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (userWithTheNameid is not null)
        {
            throw new FargoCoreException($"A user with Nameid '{nameid}' already exists.", FargoCoreErrorType.None);
        }
    }

    public static void ValidateUserCanBeDeleted(User user)
    {
        if (user.Guid == FargoCoreGuids.AdminUserGuid)
        {
            throw new FargoCoreException("The main administrator user cannot be deleted.", FargoCoreErrorType.None);
        }
    }
}
