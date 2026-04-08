namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationManager : IAuthenticationManager
{
    public AuthenticationManager(IAuthenticationClient client, AuthSession session)
    {
        this.client = client;
        this.session = session;
    }

    private readonly IAuthenticationClient client;

    private readonly AuthSession session;

    private string currentNameid = string.Empty;

    private Timer? refreshTimer = null;

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public event EventHandler<LoggedOutEventArgs>? LoggedOut;

    public event EventHandler<RefreshedEventArgs>? Refreshed;

    public event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    public async Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var result = await client.LogInAsync(nameid, password, cancellationToken);

        session.SetTokens(result.AccessToken, result.RefreshToken, result.ExpiresAt);

        currentNameid = nameid;

        ScheduleRefresh();

        LoggedIn?.Invoke(this, new LoggedInEventArgs(nameid));

        return result;
    }

    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        if (!session.IsAuthenticated)
        {
            return;
        }

        var nameid = currentNameid;
        var refreshToken = session.RefreshToken;

        session.Clear();
        currentNameid = string.Empty;

        await client.LogOutAsync(refreshToken);

        LoggedOut?.Invoke(this, new LoggedOutEventArgs(nameid));
    }

    public async Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var result = await client.Refresh(session.RefreshToken, cancellationToken);

        session.SetTokens(result.AccessToken, result.RefreshToken, result.ExpiresAt);

        ScheduleRefresh();

        Refreshed?.Invoke(this, new RefreshedEventArgs(currentNameid));

        return result;
    }

    public async Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        await client.ChangePassword(newPassword, currentPassword);

        PasswordChanged?.Invoke(this, new PasswordChangedEventArgs(currentNameid));
    }

    private void ScheduleRefresh()
    {
        var refreshIn = session.ExpiresAt - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(2);

        if (refreshIn < TimeSpan.Zero) refreshIn = TimeSpan.Zero;

        refreshTimer?.Dispose();

        refreshTimer = new Timer(async _ => await RefreshAsync(), null, refreshIn, Timeout.InfiniteTimeSpan);
    }
}
