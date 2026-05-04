using Fargo.Api.Articles;
using Fargo.Api.Authentication;
using Fargo.Api.Events;
using Fargo.Api.Items;
using Fargo.Api.Partitions;
using Fargo.Api.UserGroups;
using Fargo.Api.Users;
using Fargo.Web.Components.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Web.Components;

public static class WebComponentsServiceCollectionExtensions
{
    public static IServiceCollection AddFargoWebComponents(this IServiceCollection services)
    {
        services.AddScoped<BrowserSdkSessionStore>();
        services.AddScoped<ISessionStore>(sp => sp.GetRequiredService<BrowserSdkSessionStore>());
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IArticleManager, ArticleManager>();
        services.AddScoped<IItemManager, ItemManager>();
        services.AddScoped<IPartitionManager, PartitionManager>();
        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<IUserService>(sp => sp.GetRequiredService<IUserManager>());
        services.AddScoped<IUserGroupManager, UserGroupManager>();
        services.AddScoped<FargoHubLifetimeService>();

        return services;
    }
}
