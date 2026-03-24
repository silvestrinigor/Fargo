using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IUserRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
/// <remarks>
/// These helpers encapsulate common patterns such as retrieving entities
/// and ensuring their existence, promoting consistency and reducing
/// repetitive null-check logic across the application layer.
/// </remarks>
public static class UserRepositoryExtensions
{
    extension(IUserRepository repository)
    {
        /// <summary>
        /// Retrieves a <see cref="User"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="userGuid">
        /// The unique identifier of the user.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="User"/> associated with the specified GUID.
        /// </returns>
        /// <exception cref="UserNotFoundFargoApplicationException">
        /// Thrown when no user is found with the specified GUID.
        /// </exception>
        /// <remarks>
        /// This method follows a fail-fast approach by throwing an exception
        /// when the requested entity does not exist, eliminating the need
        /// for null checks in the calling code.
        /// </remarks>
        public async Task<User> GetFoundByGuid(
            Guid userGuid,
            CancellationToken cancellationToken = default
        )
        {
            var user = await repository.GetByGuid(userGuid, cancellationToken)
                ?? throw new UserNotFoundFargoApplicationException(userGuid);

            return user;
        }
    }
}
