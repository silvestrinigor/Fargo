using Fargo.Sdk.Authentication;
using Fargo.Sdk.Events;
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
        services.AddScoped<FargoHubLifetimeService>();

        return services;
    }
}
