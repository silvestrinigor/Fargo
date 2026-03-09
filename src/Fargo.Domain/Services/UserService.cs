using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    /// <summary>
    /// Provides domain validation and business rules
    /// related to <see cref="User"/> entities.
    /// </summary>
    public class UserService(
            IUserRepository userRepository
            )
    {
        /// <summary>
        /// Validates the rules required to create a new <see cref="User"/>.
        ///
        /// This validation ensures that the <see cref="User.Nameid"/>
        /// is unique within the system.
        /// </summary>
        /// <param name="user">
        /// The user being created.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="UserNameidAlreadyExistsDomainException">
        /// Thrown when another user with the same <see cref="User.Nameid"/> already exists.
        /// </exception>
        public async Task ValidateUserCreate(
                User user,
                CancellationToken cancellationToken = default
                )
        {
            var alreadyExistsWithNameid =
                await userRepository.ExistsByNameid(user.Nameid, cancellationToken);

            if (alreadyExistsWithNameid)
            {
                throw new UserNameidAlreadyExistsDomainException(user.Nameid);
            }
        }
    }
}