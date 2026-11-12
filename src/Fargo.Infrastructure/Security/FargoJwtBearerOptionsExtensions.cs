using Fargo.Core.Entities.Actors;
using Fargo.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Fargo.Infrastructure.Security;

public static class FargoJwtBearerOptionsExtensions
{
    public static void UseFargoTokenValidation(this JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var subjectGuidClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var authVersionClaim = context.Principal?.FindFirstValue(FargoJwtClaims.AuthVersion);
                var actorTypeClaim = context.Principal?.FindFirstValue(FargoJwtClaims.ActorType);

                if (!Guid.TryParse(subjectGuidClaim, out var subjectGuid) ||
                    !Guid.TryParse(authVersionClaim, out var authVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                var actorType = string.Equals(actorTypeClaim, FargoJwtClaims.ActorTypeService, StringComparison.Ordinal)
                    ? ActorType.Service
                    : ActorType.User;

                // Uncached on purpose, so deactivation and token revocation take effect on the next
                // request. The reader keeps the result for the request, so resolving the actor later
                // does not read it again.
                var state = await context.HttpContext.RequestServices
                    .GetRequiredService<ActorAuthorizationStateReader>()
                    .GetAsync(subjectGuid, actorType, context.HttpContext.RequestAborted);

                if (state is null || !state.IsActive || state.AuthVersion != authVersion)
                {
                    context.Fail("Token is no longer valid.");
                }
            }
        };
    }
}
