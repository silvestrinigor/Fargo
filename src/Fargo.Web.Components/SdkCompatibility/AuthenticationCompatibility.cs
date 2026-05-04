using Fargo.Sdk.Contracts;
using Fargo.Sdk.Contracts.Authentication;

namespace Fargo.Api.Authentication;

public sealed record StoredSession(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    string Nameid);

public interface ISessionStore
{
    Task SaveAsync(StoredSession session, CancellationToken cancellationToken = default);

    Task<StoredSession?> LoadAsync(CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}

public interface IAuthSession
{
    string AccessToken { get; }

    string RefreshToken { get; }

    DateTimeOffset ExpiresAt { get; }

    bool IsAdmin { get; }

    IReadOnlyCollection<ActionType> PermissionActions { get; }

    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    string Nameid { get; }
}

public sealed record AuthSession(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    string Nameid) : IAuthSession
{
    public static AuthSession Empty { get; } = new(
        string.Empty,
        string.Empty,
        DateTimeOffset.MinValue,
        false,
        [],
        [],
        string.Empty);

    public StoredSession ToStored() => new(
        AccessToken,
        RefreshToken,
        ExpiresAt,
        IsAdmin,
        PermissionActions,
        PartitionAccesses,
        Nameid);

    public static AuthSession FromStored(StoredSession stored) => new(
        stored.AccessToken,
        stored.RefreshToken,
        stored.ExpiresAt,
        stored.IsAdmin,
        stored.PermissionActions,
        stored.PartitionAccesses,
        stored.Nameid);

    public static AuthSession FromDto(AuthDto dto, string nameid) => new(
        dto.AccessToken,
        dto.RefreshToken,
        dto.ExpiresAt,
        dto.IsAdmin,
        dto.PermissionActions,
        dto.PartitionAccesses,
        nameid);
}

public interface IAuthenticationService
{
    bool IsAuthenticated { get; }

    bool IsExpired { get; }

    IAuthSession Session { get; }

    event EventHandler? LoggedIn;

    event EventHandler? LoggedOut;

    event EventHandler? Refreshed;

    Task LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    Task RestoreAsync(CancellationToken cancellationToken = default);

    Task LogOutAsync(CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken cancellationToken = default);
}

public sealed class AuthenticationService(
    IAuthenticationHttpClient client,
    FargoSdkOptions options,
    ISessionStore sessionStore) : IAuthenticationService
{
    private AuthSession session = AuthSession.Empty;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(session.AccessToken);

    public bool IsExpired => IsAuthenticated && session.ExpiresAt <= DateTimeOffset.UtcNow;

    public IAuthSession Session => session;

    public event EventHandler? LoggedIn;

    public event EventHandler? LoggedOut;

    public event EventHandler? Refreshed;

    public async Task LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var response = await client.LogInAsync(nameid, password, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowAuthFailure(response.Error);
        }

        SetSession(AuthSession.FromDto(response.Data!, nameid));
        await sessionStore.SaveAsync(session.ToStored(), cancellationToken);
        LoggedIn?.Invoke(this, EventArgs.Empty);
    }

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sessionStore.LoadAsync(cancellationToken);
        if (stored is null)
        {
            ClearSession();
            return;
        }

        SetSession(AuthSession.FromStored(stored));

        if (IsExpired && !string.IsNullOrWhiteSpace(session.RefreshToken))
        {
            var response = await client.Refresh(session.RefreshToken, cancellationToken);
            if (response.IsSuccess)
            {
                SetSession(AuthSession.FromDto(response.Data!, session.Nameid));
                await sessionStore.SaveAsync(session.ToStored(), cancellationToken);
                Refreshed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await LogOutAsync(cancellationToken);
            }
        }
    }

    public async Task LogOutAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = session.RefreshToken;
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _ = await client.LogOutAsync(refreshToken, cancellationToken);
        }

        await sessionStore.ClearAsync(cancellationToken);
        ClearSession();
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    public async Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        var response = await client.ChangePassword(newPassword, currentPassword, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowAuthFailure(response.Error);
        }

        await LogOutAsync(cancellationToken);
    }

    private void SetSession(AuthSession value)
    {
        session = value;
        options.AccessToken = value.AccessToken;
    }

    private void ClearSession()
    {
        session = AuthSession.Empty;
        options.AccessToken = null;
    }

    private static void ThrowAuthFailure(FargoSdkError? error)
    {
        if (error?.Type == FargoSdkErrorType.InvalidCredentials)
        {
            throw new InvalidCredentialsFargoSdkException(error.Detail);
        }

        if (error?.Type == FargoSdkErrorType.PasswordChangeRequired)
        {
            throw new PasswordChangeRequiredFargoSdkException(error.Detail);
        }

        throw new SdkOperationException(error, "Authentication failed.");
    }
}
