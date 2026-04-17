using Fargo.Application.Exceptions;
using Fargo.Domain;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides authorization-related extension methods for <see cref="ActorService"/>.
/// </summary>
/// <remarks>
/// These extensions centralize common validation logic for retrieving actors,
/// ensuring they exist and are in a valid state before being used in application operations.
/// </remarks>
public static class ActorServiceExtensions
{
    extension(ActorService service)
    {
        /// <summary>
        /// Retrieves an <see cref="Actor"/> by its GUID and ensures it is valid and active.
        /// </summary>
        /// <param name="actorGuid">
        /// The unique identifier of the actor.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A valid and active <see cref="Actor"/> instance.
        /// </returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the actor does not exist or is not active.
        /// </exception>
        /// <remarks>
        /// This method is typically used at the beginning of application workflows
        /// to ensure that the current actor is authenticated and eligible to perform
        /// further operations.
        /// </remarks>
        public async Task<Actor> GetAuthorizedActorByGuid(
            Guid actorGuid,
            CancellationToken cancellationToken = default
        )
        {
            var actor = await service.GetActorByGuid(actorGuid, cancellationToken);

            if (actor is null || !actor.IsActive)
            {
                throw new UnauthorizedAccessFargoApplicationException();
            }

            return actor;
        }
    }
}
