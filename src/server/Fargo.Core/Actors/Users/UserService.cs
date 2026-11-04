using Fargo.Core.Common;
using Fargo.Core.Informations;

namespace Fargo.Core.Users;

/// <summary>
/// The user core service.
/// </summary>
public sealed class UserService(IUserRepository userRepository)
{
    /// <summary>
    /// Validates that the specified <see cref="Nameid"/> is not already assigned
    /// to another user.
    /// </summary>
    /// <param name="nameid">
    /// The user identifier to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when another user already exists with the specified
    /// <see cref="Nameid"/>.
    /// </exception>
    public async Task ValidateUserNameidIsAvailableAsync(Nameid nameid, CancellationToken cancellationToken = default)
    {
        var nameidInUse = await userRepository.ExistsByNameidAsync(nameid, cancellationToken);

        if (nameidInUse)
        {
            throw new FargoCoreException($"A user with Nameid '{nameid}' already exists.", FargoErrorType.Conflict);
        }
    }
}
