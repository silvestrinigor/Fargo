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

        /// <summary>
        /// Validates the rules required to delete a <see cref="User"/>.
        ///
        /// This validation ensures that a user cannot delete their own account.
        /// </summary>
        /// <param name="user">
        /// The user being deleted.
        /// </param>
        /// <param name="actor">
        /// The user performing the delete operation.
        /// </param>
        /// <exception cref="UserCannotDeleteSelfFargoDomainException">
        /// Thrown when the acting user attempts to delete their own account.
        /// </exception>
        public static void ValidateUserDelete(User user, User actor)
        {
            if (user == actor)
            {
                throw new UserCannotDeleteSelfFargoDomainException(actor.Guid);
            }
        }

        /// <summary>
        /// Validates the rules required to change a user's permissions.
        ///
        /// This validation ensures that a user cannot modify their own permissions.
        /// </summary>
        /// <param name="user">
        /// The user whose permissions are being modified.
        /// </param>
        /// <param name="actor">
        /// The user performing the permission change operation.
        /// </param>
        /// <exception cref="UserCannotChangeOwnPermissionsFargoDomainException">
        /// Thrown when the acting user attempts to modify their own permissions.
        /// </exception>
        public static void ValidateUserPermissionChange(User user, User actor)
        {
            if (user.Guid == actor.Guid)
            {
                throw new UserCannotChangeOwnPermissionsFargoDomainException(actor.Guid);
            }
        }
    }
}