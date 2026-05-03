using Fargo.Api.ApiClients;
using Fargo.Api.Articles;
using Fargo.Api.Authentication;
using Fargo.Api.Http;
using Fargo.Api.Items;
using Fargo.Api.Partitions;
using Fargo.Api.UserGroups;
using Fargo.Api.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension methods to register the Fargo SDK simple HTTP clients in a DI container.
/// </summary>
public static class FargoSdkServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FargoSdkOptions"/>, the underlying <see cref="HttpClient"/>,
    /// and one typed HTTP client per Fargo API feature.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Delegate to configure <see cref="FargoSdkOptions"/>.</param>
    /// <param name="lifetime">
    /// Lifetime to use for the typed clients. Use <see cref="ServiceLifetime.Scoped"/> for
    /// Blazor Server (one set per circuit), <see cref="ServiceLifetime.Singleton"/> for
    /// background services and MCP tools.
    /// </param>
    public static IServiceCollection AddFargoSdk(
        this IServiceCollection services,
        Action<FargoSdkOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var options = new FargoSdkOptions();
        configure(options);
        services.TryAddSingleton(options);

        services.AddHttpClient("fargo-sdk");

        services.Add(ServiceDescriptor.Describe(
            typeof(IFargoHttpClient),
            sp => new FargoHttpClient(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("fargo-sdk"),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FargoHttpClient>>(),
                sp.GetRequiredService<FargoSdkOptions>()),
            lifetime));

        Add<IAuthenticationHttpClient, AuthenticationHttpClient>();
        Add<IArticleHttpClient, ArticleHttpClient>();
        Add<IUserHttpClient, UserHttpClient>();
        Add<IItemHttpClient, ItemHttpClient>();
        Add<IPartitionHttpClient, PartitionHttpClient>();
        Add<IUserGroupHttpClient, UserGroupHttpClient>();
        Add<IApiClientHttpClient, ApiClientHttpClient>();

        return services;

        void Add<TService, TImpl>() where TService : class where TImpl : class, TService
            => services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImpl), lifetime));
    }
}
