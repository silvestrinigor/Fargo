using Fargo.Application.Exceptions;
using Fargo.Domain.Users;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IUserGroupRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
/// <remarks>
/// These helpers encapsulate common patterns such as retrieving entities
/// and ensuring their existence, promoting consistency and reducing
/// repetitive null-check logic across the application layer.
/// </remarks>
public static class UserGroupRepositoryExtensions
{
    extension(IUserGroupRepository repository)
    {
        /// <summary>
        /// Retrieves a <see cref="UserGroup"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="userGroupGuid">
        /// The unique identifier of the user group.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="UserGroup"/> associated with the specified GUID.
        /// </returns>
        /// <exception cref="UserGroupNotFoundFargoApplicationException">
        /// Thrown when no user group is found with the specified GUID.
        /// </exception>
        /// <remarks>
        /// This method follows a fail-fast approach by throwing an exception
        /// when the requested entity does not exist, eliminating the need
        /// for null checks in the calling code.
        /// </remarks>
        public async Task<UserGroup> GetFoundByGuid(
            Guid userGroupGuid,
            CancellationToken cancellationToken = default
        )
        {
            var group = await repository.GetByGuid(userGroupGuid, cancellationToken)
                ?? throw new UserGroupNotFoundFargoApplicationException(userGroupGuid);

            return group;
        }
    }
}
