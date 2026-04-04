using Fargo.Sdk.Managers;
using Fargo.Sdk.Http;
using Fargo.Sdk.Models;
using Fargo.Sdk.Security;
using Fargo.Sdk.Services;

namespace Fargo.Sdk;

public sealed class Client : IClient, IDisposable
{
    private readonly HttpClient httpClient;
    private readonly FargoAuthSession session;
    private readonly FargoHttpClient fargoHttpClient;
    private readonly AuthService authService;

    private string currentServer = string.Empty;
    private string currentNameid = string.Empty;

    public Client()
    {
        httpClient = new HttpClient();
        session = new FargoAuthSession();
        fargoHttpClient = new FargoHttpClient(httpClient, session);
        authService = new AuthService(fargoHttpClient, session);

        Users = new UserManager(fargoHttpClient);
        Articles = new ArticleManager(fargoHttpClient);
        Items = new ItemManager(fargoHttpClient);
        Partitions = new PartitionManager(fargoHttpClient);
        UserGroups = new UserGroupManager(fargoHttpClient);
        Trees = new TreeManager(fargoHttpClient);
    }

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public event EventHandler<LoggedOutEventArgs>? LoggedOut;

    public bool IsConnected => session.IsAuthenticated;

    public IUserManager Users { get; }

    public IArticleManager Articles { get; }

    public IItemManager Items { get; }

    public IPartitionManager Partitions { get; }

    public IUserGroupManager UserGroups { get; }

    public ITreeManager Trees { get; }

    public async Task LogInAsync(string server, string nameid, string password, CancellationToken ct = default)
    {
        fargoHttpClient.SetBaseUrl(server);

        await authService.LoginAsync(nameid, password, ct);

        currentServer = server;
        currentNameid = nameid;

        LoggedIn?.Invoke(this, new LoggedInEventArgs(server, nameid));
    }

    public async Task LogOutAsync(CancellationToken ct = default)
    {
        var server = currentServer;
        var nameid = currentNameid;

        await authService.LogoutAsync(ct);

        currentServer = string.Empty;
        currentNameid = string.Empty;

        LoggedOut?.Invoke(this, new LoggedOutEventArgs(server, nameid));
    }

    public Task<AuthResult> RefreshAsync(CancellationToken ct = default)
    {
        return authService.RefreshAsync(ct);
    }

    public Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken ct = default)
    {
        return authService.ChangePasswordAsync(newPassword, currentPassword, ct);
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
