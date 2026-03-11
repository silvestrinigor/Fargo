using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    /// <summary>
    /// Provides domain validation and business rules
    /// related to <see cref="UserGroup"/> entities.
    /// </summary>
    public class UserGroupService(
            IUserGroupRepository userGroupRepository
            )
    {
        /// <summary>
        /// Validates the rules required to create a new <see cref="UserGroup"/>.
        ///
        /// This validation ensures that the <see cref="UserGroup.Nameid"/>
        /// is unique within the system.
        /// </summary>
        /// <param name="userGroup">
        /// The user group being created.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="UserGroupNameidAlreadyExistsDomainException">
        /// Thrown when another user group with the same
        /// <see cref="UserGroup.Nameid"/> already exists.
        /// </exception>
        public async Task ValidateUserGroupCreate(
                UserGroup userGroup,
                CancellationToken cancellationToken = default
                )
        {
            var alreadyExistsWithName =
                await userGroupRepository.ExistsByNameid(
                        userGroup.Nameid,
                        cancellationToken
                        );

            if (alreadyExistsWithName)
            {
                throw new UserGroupNameidAlreadyExistsDomainException(userGroup.Nameid);
            }
        }
    }
}