using Fargo.Sdk.ApiClients;
using Fargo.Sdk.Articles;
using Fargo.Sdk.Authentication;
using Fargo.Sdk.Events;
using Fargo.Sdk.Http;
using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Extensions;

/// <summary>
/// Extension methods to register the Fargo SDK services in a DI container.
/// Call <see cref="AddFargoSdk"/> for the full set, or register individual services manually
/// at whatever lifetime you need.
/// </summary>
public static class FargoSdkServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Fargo SDK services with scoped lifetime and configures the server URL.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Delegate to configure <see cref="FargoSdkOptions"/>.</param>
    /// <param name="lifetime">
    /// Lifetime to use for all registrations. Use <see cref="ServiceLifetime.Scoped"/> for
    /// Blazor Server (one set per circuit), <see cref="ServiceLifetime.Singleton"/> for
    /// background services and MCP tools.
    /// </param>
    public static IFargoSdkBuilder AddFargoSdk(
        this IServiceCollection services,
        Action<FargoSdkOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var options = new FargoSdkOptions();
        configure(options);
        services.TryAddSingleton(options);

        services.AddHttpClient();

        void Add<TService, TImpl>() where TService : class where TImpl : class, TService
            => services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImpl), lifetime));

        void AddSelf<TImpl>() where TImpl : class
            => services.Add(ServiceDescriptor.Describe(typeof(TImpl), typeof(TImpl), lifetime));

        // Auth session
        AddSelf<AuthSession>();
        services.Add(ServiceDescriptor.Describe(
            typeof(IAuthSession),
            sp => sp.GetRequiredService<AuthSession>(),
            lifetime));

        // HTTP client — circular dependency broken via Lazy<IAuthenticationService>
        services.Add(ServiceDescriptor.Describe(
            typeof(Lazy<IAuthenticationService>),
            sp => new Lazy<IAuthenticationService>(sp.GetRequiredService<IAuthenticationService>),
            lifetime));
        services.Add(ServiceDescriptor.Describe(
            typeof(IFargoHttpClient),
            sp => new FargoHttpClient(
                sp.GetRequiredService<System.Net.Http.IHttpClientFactory>().CreateClient("fargo-sdk"),
                sp.GetRequiredService<IAuthSession>(),
                sp.GetRequiredService<Lazy<IAuthenticationService>>(),
                sp.GetRequiredService<ILogger<FargoHttpClient>>(),
                sp.GetRequiredService<FargoSdkOptions>()),
            lifetime));

        // Authentication
        Add<IAuthenticationHttpClient, AuthenticationHttpClient>();
        Add<IAuthenticationService, AuthenticationService>();

        // Hub
        AddSelf<FargoEventHub>();
        services.Add(ServiceDescriptor.Describe(
            typeof(IFargoEventHub),
            sp => sp.GetRequiredService<FargoEventHub>(),
            lifetime));

        // Articles
        Add<IArticleHttpClient, ArticleHttpClient>();
        Add<IArticleService, ArticleService>();
        Add<IArticleImageService, ArticleImageService>();
        Add<IArticleBarcodeService, ArticleBarcodeService>();
        Add<IArticleEventSource, ArticleEventSource>();
        Add<IArticleManager, ArticleManager>();

        // Users
        Add<IUserHttpClient, UserHttpClient>();
        Add<IUserService, UserService>();
        Add<IUserEventSource, UserEventSource>();
        Add<IUserManager, UserManager>();

        // Items
        Add<IItemHttpClient, ItemHttpClient>();
        Add<IItemService, ItemService>();
        Add<IItemEventSource, ItemEventSource>();
        Add<IItemManager, ItemManager>();

        // Partitions
        Add<IPartitionHttpClient, PartitionHttpClient>();
        Add<IPartitionService, PartitionService>();
        Add<IPartitionEventSource, PartitionEventSource>();
        Add<IPartitionManager, PartitionManager>();

        // UserGroups
        Add<IUserGroupHttpClient, UserGroupHttpClient>();
        Add<IUserGroupService, UserGroupService>();
        Add<IUserGroupEventSource, UserGroupEventSource>();
        Add<IUserGroupManager, UserGroupManager>();

        // ApiClients
        Add<IApiClientHttpClient, ApiClientHttpClient>();
        Add<IApiClientService, ApiClientService>();
        Add<IApiClientEventSource, ApiClientEventSource>();
        Add<IApiClientManager, ApiClientManager>();

        return new FargoSdkBuilder(services);
    }

    private sealed class FargoSdkBuilder(IServiceCollection services) : IFargoSdkBuilder
    {
        public IServiceCollection Services => services;

        public IFargoSdkBuilder WithHubLifetime()
        {
            services.AddScoped<FargoHubLifetimeService>();
            return this;
        }

        public IFargoSdkBuilder WithSessionStore<TStore>() where TStore : class, ISessionStore
        {
            services.TryAddScoped<TStore>();
            services.AddScoped<ISessionStore>(sp => sp.GetRequiredService<TStore>());
            return this;
        }
    }
}
