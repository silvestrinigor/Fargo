using Fargo.Api.Articles;
using Fargo.Api.Authentication;
using Fargo.Api.Events;
using Fargo.Api.Http;
using Fargo.Api.Items;
using Fargo.Api.Partitions;
using Fargo.Api.UserGroups;
using Fargo.Api.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.Api;

/// <summary>
/// Manual composition root for non-DI scenarios (scripts, MCP, desktop apps).
/// Create one instance per application and dispose it on shutdown.
/// </summary>
public sealed class Engine : IEngine
{
    public Engine(ILoggerFactory? loggerFactory = null, ISessionStore? sessionStore = null)
    {
        options = new FargoSdkOptions();

        httpClient = new HttpClient();

        var authSession = new AuthSession();
        this.authSession = authSession;

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

        SubscribeHubLifecycle();
    }

    internal Engine(
        FargoSdkOptions options,
        IAuthenticationService authentication,
        IArticleManager articles,
        IUserManager users,
        IItemManager items,
        IPartitionManager partitions,
        IUserGroupManager userGroups,
        IFargoEventHub hub)
    {
        this.options = options;
        Authentication = authentication;
        Articles = articles;
        Users = users;
        Items = items;
        Partitions = partitions;
        UserGroups = userGroups;
        _hub = hub;
        authSession = authentication.Session;
        SubscribeHubLifecycle();
    }

    public IAuthenticationService Authentication { get; }
    public IArticleManager Articles { get; }
    public IUserManager Users { get; }
    public IItemManager Items { get; }
    public IPartitionManager Partitions { get; }
    public IUserGroupManager UserGroups { get; }

    public void Configure(string server)
        => options.Server = server;

    /// <summary>Logs in using the configured server.</summary>
    public async Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default)
    {
        Configure(server);
        await Authentication.LogInAsync(nameid, password, cancellationToken);
    }

    /// <summary>Logs out and lets the hub lifecycle subscription disconnect the event hub.</summary>
    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        await Authentication.LogOutAsync(cancellationToken);
    }

    /// <summary>
    /// Restores a saved session without re-authenticating.
    /// Returns <see langword="false"/> if no session store is configured or no saved session exists.
    /// </summary>
    public async Task<bool> RestoreSessionAsync(string server, CancellationToken cancellationToken = default)
    {
        Configure(server);
        return await Authentication.RestoreAsync(cancellationToken);
    }

    public void Dispose()
    {
        Authentication.LoggedIn -= OnLoggedIn;
        Authentication.LoggedOut -= OnLoggedOut;
        Authentication.Restored -= OnRestored;
        _hub.DisposeAsync().AsTask().GetAwaiter().GetResult();
        httpClient?.Dispose();
    }

    private void SubscribeHubLifecycle()
    {
        Authentication.LoggedIn += OnLoggedIn;
        Authentication.LoggedOut += OnLoggedOut;
        Authentication.Restored += OnRestored;
    }

    private void OnLoggedIn(object? sender, LoggedInEventArgs e)
        => _ = _hub.ConnectAsync(options.Server, () => Task.FromResult(authSession.AccessToken));

    private void OnRestored(object? sender, SessionRestoredEventArgs e)
        => _ = _hub.ConnectAsync(options.Server, () => Task.FromResult(authSession.AccessToken));

    private void OnLoggedOut(object? sender, LoggedOutEventArgs e)
        => _ = _hub.DisconnectAsync();

    private readonly FargoSdkOptions options;
    private readonly IAuthSession authSession;
    private readonly IFargoEventHub _hub;
    private readonly HttpClient? httpClient;
}
