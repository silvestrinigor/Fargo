using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Authentication;

/// <summary>
/// Default implementation of <see cref="IAuthenticationService"/>. Performs inline token refresh
/// (via <see cref="FargoHttpClient"/>) instead of a background timer.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    /// <summary>Initializes a new instance.</summary>
    public AuthenticationService(
        IAuthenticationHttpClient client,
        AuthSession session,
        ILogger<AuthenticationService> logger,
        ISessionStore? sessionStore = null)
    {
        this.client = client;
        this.session = session;
        this.logger = logger;
        this.sessionStore = sessionStore;
    }

    private readonly IAuthenticationHttpClient client;
    private readonly AuthSession session;
    private readonly ILogger logger;
    private readonly ISessionStore? sessionStore;

    /// <inheritdoc />
    public event EventHandler<LoggedInEventArgs>? LoggedIn;
    /// <inheritdoc />
    public event EventHandler<LoggedOutEventArgs>? LoggedOut;
    /// <inheritdoc />
    public event EventHandler<RefreshedEventArgs>? Refreshed;
    /// <inheritdoc />
    public event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    public IAuthSession Session => session;
    public bool IsAuthenticated => session.IsAuthenticated;
    public bool IsExpired => session.IsExpired;

    /// <inheritdoc />
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

        session.SetTokens(nameid, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt, result.Data.IsAdmin, result.Data.PermissionActions, result.Data.PartitionAccesses);

        if (sessionStore is not null)
        {
            await sessionStore.SaveAsync(ToStoredSession(), cancellationToken);
        }

        logger.LogLoggedIn(nameid);
        LoggedIn?.Invoke(this, new LoggedInEventArgs(nameid));

        return result.Data;
    }

    /// <inheritdoc />
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

        if (sessionStore is not null)
        {
            await sessionStore.ClearAsync(cancellationToken);
        }

        logger.LogLoggedOut(nameid!);
        LoggedOut?.Invoke(this, new LoggedOutEventArgs(nameid!));
    }

    /// <inheritdoc />
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

        session.SetTokens(session.Nameid!, result.Data!.AccessToken, result.Data.RefreshToken, result.Data.ExpiresAt, result.Data.IsAdmin, result.Data.PermissionActions, result.Data.PartitionAccesses);

        if (sessionStore is not null)
        {
            await sessionStore.SaveAsync(ToStoredSession(), cancellationToken);
        }

        logger.LogRefreshed(session.Nameid!);
        Refreshed?.Invoke(this, new RefreshedEventArgs(session.Nameid!));

        return result.Data;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        session.SetTokens(stored.Nameid, stored.AccessToken, stored.RefreshToken, stored.ExpiresAt, stored.IsAdmin, stored.PermissionActions, stored.PartitionAccesses);

        if (IsExpired)
        {
            await RefreshAsync(cancellationToken);
        }

        return true;
    }

    private StoredSession ToStoredSession() =>
        new(session.Nameid!, session.AccessToken!, session.RefreshToken!, session.ExpiresAt!.Value, session.IsAdmin, session.PermissionActions, session.PartitionAccesses);

    private static void ThrowAuthError(FargoSdkError error) =>
        throw error.Type switch
        {
            FargoSdkErrorType.InvalidCredentials => (FargoSdkException)new InvalidCredentialsFargoSdkException(error.Detail),
            FargoSdkErrorType.PasswordChangeRequired => new PasswordChangeRequiredFargoSdkException(error.Detail),
            _ => new FargoSdkApiException(error.Detail)
        };
}
