using Fargo.Sdk.Articles;
using Fargo.Sdk.Authentication;
using Fargo.Sdk.Events;
using Fargo.Sdk.Http;
using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.Sdk;

/// <summary>
/// Manual composition root for non-DI scenarios (scripts, MCP, desktop apps).
/// Create one instance per application and dispose it on shutdown.
/// </summary>
public sealed class Engine : IDisposable
{
    public Engine(ILoggerFactory? loggerFactory = null, ISessionStore? sessionStore = null)
    {
        options = new FargoSdkOptions();

        httpClient = new HttpClient();

        authSession = new AuthSession();

        // Break circular dependency: FargoHttpClient → IAuthenticationService → AuthenticationHttpClient → IFargoHttpClient
        AuthenticationService? authService = null;
        var lazyAuth = new Lazy<IAuthenticationService>(() => authService!);

        var fargoHttp = new FargoHttpClient(
            httpClient,
            authSession,
            lazyAuth,
            (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<FargoHttpClient>(),
            options);

        authService = new AuthenticationService(
            new AuthenticationHttpClient(fargoHttp),
            authSession,
            (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<AuthenticationService>(),
            sessionStore);

        Authentication = authService;

        _hub = new FargoEventHub();

        var articleHttpClient = new ArticleHttpClient(fargoHttp);
        var articleService = new ArticleService(articleHttpClient, _hub);
        var articleImageService = new ArticleImageService(articleHttpClient);
        var articleBarcodeService = new ArticleBarcodeService(articleHttpClient);
        var articleEventSource = new ArticleEventSource(_hub);
        Articles = new ArticleManager(articleService, articleImageService, articleBarcodeService, articleEventSource);

        var userHttpClient = new UserHttpClient(fargoHttp);
        var userService = new UserService(userHttpClient, _hub);
        var userEventSource = new UserEventSource(_hub);
        Users = new UserManager(userService, userEventSource);

        var itemHttpClient = new ItemHttpClient(fargoHttp);
        var itemService = new ItemService(itemHttpClient, _hub);
        var itemEventSource = new ItemEventSource(_hub);
        Items = new ItemManager(itemService, itemEventSource);

        var partitionHttpClient = new PartitionHttpClient(fargoHttp);
        var partitionService = new PartitionService(partitionHttpClient, _hub);
        var partitionEventSource = new PartitionEventSource(_hub);
        Partitions = new PartitionManager(partitionService, partitionEventSource);

        var userGroupHttpClient = new UserGroupHttpClient(fargoHttp);
        var userGroupService = new UserGroupService(userGroupHttpClient, _hub);
        var userGroupEventSource = new UserGroupEventSource(_hub);
        UserGroups = new UserGroupManager(userGroupService, userGroupEventSource);
    }

    public IAuthenticationService Authentication { get; }
    public IArticleManager Articles { get; }
    public IUserManager Users { get; }
    public IItemManager Items { get; }
    public IPartitionManager Partitions { get; }
    public IUserGroupManager UserGroups { get; }

    /// <summary>Logs in and connects the event hub.</summary>
    public async Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default)
    {
        if (Authentication.IsAuthenticated)
        {
            await Authentication.LogOutAsync(cancellationToken);
        }

        options.Server = server;

        await Authentication.LogInAsync(nameid, password, cancellationToken);
        await _hub.ConnectAsync(server, () => Task.FromResult(authSession.AccessToken), cancellationToken);
    }

    /// <summary>Disconnects the event hub and logs out.</summary>
    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        await _hub.DisconnectAsync(cancellationToken);
        await Authentication.LogOutAsync(cancellationToken);
    }

    /// <summary>
    /// Restores a saved session without re-authenticating.
    /// Returns <see langword="false"/> if no session store is configured or no saved session exists.
    /// </summary>
    public async Task<bool> RestoreSessionAsync(string server, CancellationToken cancellationToken = default)
    {
        options.Server = server;

        var restored = await Authentication.RestoreAsync(cancellationToken);

        if (restored)
        {
            await _hub.ConnectAsync(server, () => Task.FromResult(authSession.AccessToken), cancellationToken);
        }

        return restored;
    }

    public void Dispose()
    {
        _ = _hub.DisposeAsync();
        httpClient.Dispose();
    }

    private readonly FargoSdkOptions options;
    private readonly AuthSession authSession;
    private readonly FargoEventHub _hub;
    private readonly HttpClient httpClient;
}
