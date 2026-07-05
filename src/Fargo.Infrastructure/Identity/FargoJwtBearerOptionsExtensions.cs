using Fargo.Core.Users;
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
                var userGuidClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var authVersionClaim = context.Principal?.FindFirstValue(FargoJwtClaims.AuthVersion);

                if (!Guid.TryParse(userGuidClaim, out var userGuid) ||
                    !Guid.TryParse(authVersionClaim, out var authVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetByGuidAsync(userGuid, context.HttpContext.RequestAborted);

                if (user is null || !user.IsActive || user.AuthVersion != authVersion)
                {
                    context.Fail("Token is no longer valid.");
                }
            }
        };
    }
}
