using Fargo.Core.Entities.Actors;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Shared by the HTTP request actor (<see cref="CurrentUserActor"/>) and hosts that have a
/// <see cref="ClaimsPrincipal"/> without an HTTP request, such as a SignalR hub.
/// </summary>
public static class FargoClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        /// <summary>
        /// Returns <see cref="Guid.Empty"/> when the principal is not authenticated or the claim is missing or invalid.
        /// </summary>
        public Guid GetFargoActorGuid()
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return Guid.Empty;
            }

            var id =
                principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
        }

        /// <summary>
        /// A token without the actor-type claim is a user token.
        /// </summary>
        public ActorType GetFargoActorType()
        {
            var actorTypeClaim = principal?.FindFirstValue(FargoJwtClaims.ActorType);

            return string.Equals(actorTypeClaim, FargoJwtClaims.ActorTypeService, StringComparison.Ordinal)
                ? ActorType.Service
                : ActorType.User;
        }
    }
}
