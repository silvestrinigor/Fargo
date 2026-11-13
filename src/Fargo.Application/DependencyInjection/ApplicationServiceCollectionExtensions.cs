using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Application.DependencyInjection;

/// <summary>
/// Each feature owns its handler registrations (the other *ServiceCollectionExtensions.cs files in
/// this folder); this class only composes them with the domain services they depend on.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoApplication() => services
        .AddFargoDomainServices()
        .AddFargoArticleApplication()
        .AddFargoPartitionApplication()
        .AddFargoUserGroupApplication()
        .AddFargoUserApplication()
        .AddFargoItemApplication()
        .AddFargoIdentityApplication()
        .AddFargoServiceAccountApplication()
        .AddFargoInitializationApplication()
        .AddFargoAuditLogApplication()
        .AddFargoMetadataApplication()
        .AddFargoSupplyOrderApplication()
        .AddFargoOutboxApplication()
        .AddFargoIdempotencyApplication();

        private IServiceCollection AddFargoDomainServices()
        {
            // One instance serves every consumer; handlers take the time from here so time-dependent
            // behaviour (token expiry, lockout windows) is deterministic in tests.
            services.TryAddSingleton(TimeProvider.System);

            return services
            .AddScoped<ActorResolver>()
            .AddScoped<ArticleService>()
            .AddScoped<UserService>()
            .AddScoped<UserGroupService>()
            .AddScoped<PartitionService>()
            .AddScoped<ItemService>()
            .AddScoped<IAuthenticationAttemptService, AuthenticationAttemptService>();
        }
    }
}
