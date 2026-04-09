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

    private Timer? refreshTimer = null;

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public event EventHandler<LoggedOutEventArgs>? LoggedOut;

    public event EventHandler<RefreshedEventArgs>? Refreshed;

    public event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    public bool IsAuthenticated => session.IsAuthenticated;

    public bool IsExpired => session.IsExpired;

    public async Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        if (IsAuthenticated)
        {
            await LogOutAsync(cancellationToken);
        }

        var result = await client.LogInAsync(nameid, password, cancellationToken);

        session.SetTokens(nameid, result.AccessToken, result.RefreshToken, result.ExpiresAt);

        ScheduleRefresh();

        LoggedIn?.Invoke(this, new LoggedInEventArgs(nameid));

        return result;
    }

    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated) return;

        var refreshToken = session.RefreshToken;

        if (refreshToken is null)
        {
            return;
        }

        await client.LogOutAsync(refreshToken, cancellationToken);

        var nameid = session.Nameid;

        session.Clear();

        LoggedOut?.Invoke(this, new LoggedOutEventArgs(nameid!));
    }

    public async Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        var result = await client.Refresh(session.RefreshToken!, cancellationToken);

        session.SetTokens(session.Nameid!, result.AccessToken, result.RefreshToken, result.ExpiresAt);

        ScheduleRefresh();

        Refreshed?.Invoke(this, new RefreshedEventArgs(session.Nameid!));

        return result;
    }

    public async Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        await client.ChangePassword(newPassword, currentPassword, cancellationToken);

        PasswordChanged?.Invoke(this, new PasswordChangedEventArgs(session.Nameid!));
    }

    private readonly object refreshTimerLock = new();

    private void ScheduleRefresh()
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        var refreshIn = session.ExpiresAt - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(2);

        if (refreshIn < TimeSpan.Zero) refreshIn = TimeSpan.Zero;

        lock (refreshTimerLock)
        {
            refreshTimer?.Dispose();

            refreshTimer = new Timer(async _ =>
                await AutoRefreshAsync(), null, refreshIn!.Value, Timeout.InfiniteTimeSpan);
        }
    }

    private async Task AutoRefreshAsync()
    {
        try
        {
            await RefreshAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void Dispose()
    {
        lock (refreshTimerLock)
        {
            refreshTimer?.Dispose();
            refreshTimer = null;
        }
    }
}
