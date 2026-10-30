using Fargo.Core.Identity;
using Fargo.Core.ServiceAccounts;
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
                var subjectGuidClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var authVersionClaim = context.Principal?.FindFirstValue(FargoJwtClaims.AuthVersion);
                var actorTypeClaim = context.Principal?.FindFirstValue(FargoJwtClaims.ActorType);

                if (!Guid.TryParse(subjectGuidClaim, out var subjectGuid) ||
                    !Guid.TryParse(authVersionClaim, out var authVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                var isServiceToken = string.Equals(
                    actorTypeClaim, FargoJwtClaims.ActorTypeService, StringComparison.Ordinal);

                if (isServiceToken)
                {
                    var serviceAccountRepository = context.HttpContext.RequestServices
                        .GetRequiredService<IServiceAccountRepository>();

                    var service = await serviceAccountRepository.GetByGuidAsync(
                        subjectGuid, context.HttpContext.RequestAborted);

                    if (service is null || !service.IsActive || service.AuthVersion != authVersion)
                    {
                        context.Fail("Token is no longer valid.");
                    }

                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetByGuidAsync(subjectGuid, context.HttpContext.RequestAborted);

                if (user is null || !user.IsActive || user.Authentication.AuthVersion != authVersion)
                {
                    context.Fail("Token is no longer valid.");
                }
            }
        };
    }
}