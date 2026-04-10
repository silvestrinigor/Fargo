using Fargo.Sdk.Articles;
using Fargo.Sdk.Authentication;
using Fargo.Sdk.Http;
using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.Sdk;

public sealed class Engine : IEngine
{
    public Engine(ILoggerFactory? loggerFactory = null, ISessionStore? sessionStore = null)
    {
        httpClient = new HttpClient();

        var session = new AuthSession();

        var logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<FargoSdkHttpClient>();

        fargoHttpClient = new FargoSdkHttpClient(httpClient, session, logger);

        var authClient = new AuthenticationClient(fargoHttpClient);

        var authLogger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<AuthenticationManager>();

        Authentication = new AuthenticationManager(authClient, session, authLogger, sessionStore);
        Users = new UserClient(fargoHttpClient);
        UserGroups = new UserGroupClient(fargoHttpClient);
        Articles = new ArticleClient(fargoHttpClient);
        Items = new ItemClient(fargoHttpClient);
        Partitions = new PartitionClient(fargoHttpClient);
    }

    public IAuthenticationManager Authentication { get; }

    public IUserClient Users { get; }

    public IUserGroupClient UserGroups { get; }

    public IArticleClient Articles { get; }

    public IItemClient Items { get; }

    public IPartitionClient Partitions { get; }

    /// <summary>
    /// Configures the server URL without performing any authentication.
    /// Use this in hosted scenarios where the server address is known upfront
    /// (e.g. read from configuration) and authentication is managed separately
    /// via <see cref="IAuthenticationManager"/>.
    /// </summary>
    public void Configure(string server) => fargoHttpClient.SetBaseUrl(server);

    public async Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default)
    {
        if (Authentication.IsAuthenticated)
        {
            await Authentication.LogOutAsync(cancellationToken);
        }

        fargoHttpClient.SetBaseUrl(server);

        await Authentication.LogInAsync(nameid, password, cancellationToken);
    }

    public Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        return Authentication.LogOutAsync(cancellationToken);
    }

    public async Task<bool> RestoreSessionAsync(string server, CancellationToken cancellationToken = default)
    {
        fargoHttpClient.SetBaseUrl(server);

        return await Authentication.RestoreAsync(cancellationToken);
    }

    public void Dispose()
    {
        Authentication.Dispose();
        httpClient.Dispose();
    }

    private readonly HttpClient httpClient;

    private readonly FargoSdkHttpClient fargoHttpClient;
}
