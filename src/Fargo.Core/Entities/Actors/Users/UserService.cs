using Fargo.Core.Common;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Actors.Users;

public sealed class UserService(IUserRepository userRepository)
{
    /// <exception cref="FargoCoreException">Another user already uses the <see cref="Nameid"/>.</exception>
    public async Task ValidateUserNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var nameidInUse = await userRepository.ExistsByNameidAsync(nameid, cancellationToken);

        if (nameidInUse)
        {
            throw new FargoCoreException($"A user with Nameid '{nameid}' already exists.", FargoErrorType.Conflict);
        }
    }
}
