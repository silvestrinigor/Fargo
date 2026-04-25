namespace Fargo.Sdk.Authentication;

/// <summary>
/// Manages user authentication lifecycle, including login, logout, token refresh, and password changes.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>Raised when the user successfully logs in.</summary>
    event EventHandler<LoggedInEventArgs>? LoggedIn;

    /// <summary>Raised when the user logs out.</summary>
    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    /// <summary>Raised when the authentication token is successfully refreshed.</summary>
    event EventHandler<RefreshedEventArgs>? Refreshed;

    /// <summary>Raised when the user's password is successfully changed.</summary>
    event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    /// <summary>Gets the current authentication session.</summary>
    IAuthSession Session { get; }

    /// <summary>Gets a value indicating whether the user is currently authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets a value indicating whether the current authentication token has expired.</summary>
    bool IsExpired { get; }

    /// <summary>Authenticates the user with the provided credentials.</summary>
    Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    /// <summary>Ends the current authenticated session.</summary>
    Task LogOutAsync(CancellationToken cancellationToken = default);

    /// <summary>Refreshes the current authentication token.</summary>
    Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>Changes the authenticated user's password.</summary>
    Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a previously saved session. If the stored token is expired a refresh is attempted.
    /// Returns <see langword="false"/> if no session store is configured or no saved session exists.
    /// </summary>
    Task<bool> RestoreAsync(CancellationToken cancellationToken = default);
}
