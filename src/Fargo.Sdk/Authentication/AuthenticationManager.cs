using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationManager : IAuthenticationManager
{
    public AuthenticationManager(IAuthenticationClient client, AuthSession session, ILogger logger)
    {
        this.client = client;
        this.session = session;
        this.logger = logger;
    }

    private readonly IAuthenticationClient client;

    private readonly AuthSession session;

    private readonly ILogger logger;

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

        if (!result.IsSuccess)
        {
        }

        session.SetTokens(nameid, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt);

        ScheduleRefresh();

        logger.LogLoggedIn(nameid);

        LoggedIn?.Invoke(this, new LoggedInEventArgs(nameid));

        return result.Data;
    }

    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            return;
        }

        var refreshToken = session.RefreshToken;

        if (refreshToken is null)
        {
            return;
        }

        await client.LogOutAsync(refreshToken, cancellationToken);

        var nameid = session.Nameid;

        session.Clear();

        logger.LogLoggedOut(nameid!);

        LoggedOut?.Invoke(this, new LoggedOutEventArgs(nameid!));
    }

    public async Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        var result = await client.Refresh(session.RefreshToken!, cancellationToken);

        session.SetTokens(session.Nameid!, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt);

        ScheduleRefresh();

        logger.LogRefreshed(session.Nameid!);

        Refreshed?.Invoke(this, new RefreshedEventArgs(session.Nameid!));

        return result.Data;
    }

    public async Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        await client.ChangePassword(newPassword, currentPassword, cancellationToken);

        logger.LogPasswordChanged(session.Nameid!);

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

        if (refreshIn < TimeSpan.Zero)
        {
            refreshIn = TimeSpan.Zero;
        }

        logger.LogRefreshScheduled(refreshIn!.Value.TotalMinutes);

        lock (refreshTimerLock)
        {
            refreshTimer?.Dispose();

            refreshTimer = new Timer(async _ =>
                await RefreshAsync(), null, refreshIn!.Value, Timeout.InfiniteTimeSpan);
        }
    }

    public void Dispose()
    {
        lock (refreshTimerLock)
        {
            if (refreshTimer is not null)
            {
                refreshTimer.Dispose();
                refreshTimer = null;
                logger.LogRefreshCancelled();
            }
        }
    }
}
