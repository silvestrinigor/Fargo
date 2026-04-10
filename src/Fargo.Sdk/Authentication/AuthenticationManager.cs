using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationManager : IAuthenticationManager
{
    public AuthenticationManager(IAuthenticationClient client, AuthSession session, ILogger logger, ISessionStore? sessionStore = null)
    {
        this.client = client;
        this.session = session;
        this.logger = logger;
        this.sessionStore = sessionStore;
    }

    private readonly IAuthenticationClient client;

    private readonly AuthSession session;

    private readonly ILogger logger;

    private readonly ISessionStore? sessionStore;

    private CancellationTokenSource? refreshCts;

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public event EventHandler<LoggedOutEventArgs>? LoggedOut;

    public event EventHandler<RefreshedEventArgs>? Refreshed;

    public event EventHandler<RefreshFailedEventArgs>? RefreshFailed;

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
            ThrowAuthError(result.Error!);
        }

        session.SetTokens(nameid, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt);

        if (sessionStore is not null)
        {
            await sessionStore.SaveAsync(ToStoredSession(), cancellationToken);
        }

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

        CancelRefresh();

        await client.LogOutAsync(refreshToken, cancellationToken);

        var nameid = session.Nameid;

        session.Clear();

        if (sessionStore is not null)
        {
            await sessionStore.ClearAsync(cancellationToken);
        }

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

        if (!result.IsSuccess)
        {
            ThrowAuthError(result.Error!);
        }

        session.SetTokens(session.Nameid!, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt);

        if (sessionStore is not null)
        {
            await sessionStore.SaveAsync(ToStoredSession(), cancellationToken);
        }

        ScheduleRefresh();

        logger.LogRefreshed(session.Nameid!);

        Refreshed?.Invoke(this, new RefreshedEventArgs(session.Nameid!));

        return result.Data;
    }

    public async Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UserNotAuthenticatedFargoSdkException();
        }

        var result = await client.ChangePassword(newPassword, currentPassword, cancellationToken);

        if (!result.IsSuccess)
        {
            ThrowAuthError(result.Error!);
        }

        logger.LogPasswordChanged(session.Nameid!);

        PasswordChanged?.Invoke(this, new PasswordChangedEventArgs(session.Nameid!));
    }

    public async Task<bool> RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (sessionStore is null)
        {
            return false;
        }

        var stored = await sessionStore.LoadAsync(cancellationToken);

        if (stored is null)
        {
            return false;
        }

        session.SetTokens(stored.Nameid, stored.AccessToken, stored.RefreshToken, stored.ExpiresAt);

        if (IsExpired)
        {
            await RefreshAsync(cancellationToken);
        }
        else
        {
            ScheduleRefresh();
        }

        return true;
    }

    public void Dispose()
    {
        CancelRefresh();
        logger.LogRefreshCancelled();
    }

    private void ScheduleRefresh()
    {
        var delay = session.ExpiresAt.GetValueOrDefault() - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(2);

        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        logger.LogRefreshScheduled(delay.TotalMinutes);

        CancelRefresh();

        refreshCts = new CancellationTokenSource();

        _ = RefreshAfterDelayAsync(delay, refreshCts.Token);
    }

    private async Task RefreshAfterDelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
            await RefreshAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // expected on logout or dispose
        }
        catch (Exception ex)
        {
            logger.LogRefreshFailed(session.Nameid, ex);
            RefreshFailed?.Invoke(this, new RefreshFailedEventArgs(session.Nameid, ex));
        }
    }

    private void CancelRefresh()
    {
        refreshCts?.Cancel();
        refreshCts?.Dispose();
        refreshCts = null;
    }

    private StoredSession ToStoredSession() =>
        new(session.Nameid!, session.AccessToken!, session.RefreshToken!, session.ExpiresAt!.Value);

    private static void ThrowAuthError(FargoSdkError error) =>
        throw error.Type switch
        {
            FargoSdkErrorType.InvalidCredentials => (FargoSdkException)new InvalidCredentialsFargoSdkException(error.Detail),
            FargoSdkErrorType.PasswordChangeRequired => new PasswordChangeRequiredFargoSdkException(error.Detail),
            _ => new FargoSdkApiException(error.Detail)
        };
}
