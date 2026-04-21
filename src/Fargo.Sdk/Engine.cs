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
/// The default implementation of <see cref="IEngine"/>. Create one instance per application and dispose it on shutdown.
/// </summary>
public sealed class Engine : IEngine
{
    /// <inheritdoc/>
    public Engine(ILoggerFactory? loggerFactory = null, ISessionStore? sessionStore = null)
    {
        httpClient = new HttpClient();

        session = new AuthSession();

        var logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<FargoSdkHttpClient>();

        fargoHttpClient = new FargoSdkHttpClient(httpClient, session, logger);

        var authClient = new AuthenticationClient(fargoHttpClient);

        var authLogger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<AuthenticationManager>();

        Authentication = new AuthenticationManager(authClient, session, authLogger, sessionStore);
        hubConnection = new FargoHubConnection();
        Users = new UserManager(new UserClient(fargoHttpClient), hubConnection);
        UserGroups = new UserGroupManager(new UserGroupClient(fargoHttpClient), hubConnection);
        Articles = new ArticleManager(new ArticleClient(fargoHttpClient), hubConnection);
        Items = new ItemManager(new ItemClient(fargoHttpClient), hubConnection);
        Partitions = new PartitionManager(new PartitionClient(fargoHttpClient), hubConnection);
    }

    /// <inheritdoc/>
    public IAuthenticationManager Authentication { get; }

    /// <inheritdoc/>
    public IUserManager Users { get; }

    /// <inheritdoc/>
    public IUserGroupManager UserGroups { get; }

    /// <inheritdoc/>
    public IArticleManager Articles { get; }

    /// <inheritdoc/>
    public IItemManager Items { get; }

    /// <inheritdoc/>
    public IPartitionManager Partitions { get; }

    /// <summary>
    /// Configures the server URL without performing any authentication.
    /// Use this in hosted scenarios where the server address is known upfront
    /// (e.g. read from configuration) and authentication is managed separately
    /// via <see cref="IAuthenticationManager"/>.
    /// </summary>
    public void Configure(string server) => fargoHttpClient.SetBaseUrl(server);

    /// <inheritdoc/>
    public async Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default)
    {
        if (Authentication.IsAuthenticated)
        {
            await Authentication.LogOutAsync(cancellationToken);
        }

        fargoHttpClient.SetBaseUrl(server);

        await Authentication.LogInAsync(nameid, password, cancellationToken);

        await hubConnection.ConnectAsync(server, () => Task.FromResult(session.AccessToken), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        await hubConnection.DisconnectAsync(cancellationToken);

        await Authentication.LogOutAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> RestoreSessionAsync(string server, CancellationToken cancellationToken = default)
    {
        fargoHttpClient.SetBaseUrl(server);

        var restored = await Authentication.RestoreAsync(cancellationToken);

        if (restored)
        {
            await hubConnection.ConnectAsync(server, () => Task.FromResult(session.AccessToken), cancellationToken);
        }

        return restored;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Authentication.Dispose();
        _ = hubConnection.DisposeAsync();
        httpClient.Dispose();
    }

    private readonly HttpClient httpClient;

    private readonly FargoSdkHttpClient fargoHttpClient;

    private readonly AuthSession session;

    private readonly FargoHubConnection hubConnection;
}
