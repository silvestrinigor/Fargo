using Fargo.Application.Operations.Articles;
using Fargo.Application.Operations.Audits;
using Fargo.Application.Operations.Identity;
using Fargo.Application.Operations.Initialization;
using Fargo.Application.Operations.Items;
using Fargo.Application.Operations.Partitions;
using Fargo.Application.Operations.ServiceAccounts;
using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Operations.UserGroups;
using Fargo.Application.Operations.Users;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

/// <summary>
/// Entry point for registering the Fargo application layer. Each feature owns its
/// handler registrations (see <c>Operations/&lt;Feature&gt;/*ServiceCollectionExtensions.cs</c>);
/// this class only composes them together with the domain services they depend on.
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
        .AddFargoSupplyOrderApplication();

        public IServiceCollection AddFargoDomainServices() => services
        .AddScoped<ActorResolver>()
        .AddScoped<ArticleService>()
        .AddScoped<UserService>()
        .AddScoped<UserGroupService>()
        .AddScoped<PartitionService>()
        .AddScoped<ItemService>()
        .AddScoped<IAuthenticationAttemptService, AuthenticationAttemptService>();
    }
}
