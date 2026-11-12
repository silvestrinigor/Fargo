using Fargo.Application.Common;
using Fargo.Application.Common.Actors;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Infrastructure.Client;
using Fargo.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

/// <summary>
/// Registers credentials, tokens, JWT options and the request-bound actor and
/// client context.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddFargoSecurityInfrastructure(IConfiguration configuration) => services
        .AddFargoJwtOptions(configuration)
        .AddFargoSecurity()
        .AddFargoRequestContext();

        private IServiceCollection AddFargoJwtOptions(IConfiguration configuration)
        {
            services.ConfigureOptions<JwtBearerOptionsSetup>();

            // ValidateDataAnnotations enforces the Key/Issuer/Audience rules on
            // first resolution at runtime (during authentication setup at
            // startup), so a missing or weak secret fails fast. ValidateOnStart
            // is intentionally not used: it would force validation during
            // build-time OpenAPI generation and design-time tooling, which run
            // without the Development/secret configuration loaded.
            services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations();

            return services;
        }

        private IServiceCollection AddFargoSecurity() => services
        .AddScoped<IPasswordHasher, IdentityPasswordHasher>()
        .AddScoped<ITokenGenerator, JwtTokenGenerator>()
        .AddScoped<ITokenHasher, Sha256TokenHasher>()
        .AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>()
        .AddScoped<IClientSecretGenerator, ClientSecretGenerator>();

        private IServiceCollection AddFargoRequestContext()
        {
            services.AddHttpContextAccessor();

            return services
            .AddScoped<ICurrentActor, CurrentUserActor>()
            .AddScoped<IClientContext, HttpClientContext>();
        }
    }
}
