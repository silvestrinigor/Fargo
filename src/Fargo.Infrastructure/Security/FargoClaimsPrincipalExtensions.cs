using Fargo.Core.Entities.Actors;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Reads the Fargo actor identity from a validated token's claims.
/// </summary>
/// <remarks>
/// Shared by the HTTP request actor (<see cref="CurrentUserActor"/>) and hosts
/// that have a <see cref="ClaimsPrincipal"/> without a current HTTP request,
/// such as a SignalR hub.
/// </remarks>
public static class FargoClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        /// <summary>
        /// Gets the actor identifier from the subject claim, or
        /// <see cref="Guid.Empty"/> when the principal is not authenticated or
        /// the claim is missing or invalid.
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
        /// Gets the actor type from the actor-type claim; a token without it is a
        /// user token.
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
