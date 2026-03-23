using Fargo.Application.Exceptions;
using Fargo.Domain.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ActorService"/> related to authorization.
/// </summary>
public static class ActorServiceExtensions
{
    extension(ActorService service)
    {
        /// <summary>
        /// Retrieves a <see cref="UserActor"/> by its GUID and ensures the user is authorized and active.
        /// </summary>
        /// <param name="userGuid">The unique identifier of the user.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A valid and active <see cref="UserActor"/> instance.
        /// </returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the user does not exist or is not active.
        /// </exception>
        public async Task<UserActor> GetAuthorizedUserActorByGuid(Guid userGuid, CancellationToken cancellationToken = default)
        {
            var actor = await service.GetUserActorByGuid(userGuid, cancellationToken);

            if (actor is null || !actor.User.IsActive)
            {
                throw new UnauthorizedAccessFargoApplicationException();
            }

            return actor;
        }
    }
}
