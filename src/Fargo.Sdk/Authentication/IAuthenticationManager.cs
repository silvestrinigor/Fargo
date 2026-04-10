namespace Fargo.Sdk.Authentication;

/// <summary>
/// Manages user authentication lifecycle, including login, logout, token refresh, and password changes.
/// </summary>
public interface IAuthenticationManager : IDisposable
{
    /// <summary>Raised when the user successfully logs in.</summary>
    event EventHandler<LoggedInEventArgs>? LoggedIn;

    /// <summary>Raised when the user logs out.</summary>
    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    /// <summary>Raised when the authentication token is successfully refreshed.</summary>
    event EventHandler<RefreshedEventArgs>? Refreshed;

    /// <summary>Raised when the user's password is successfully changed.</summary>
    event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    /// <summary>Gets a value indicating whether the user is currently authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets a value indicating whether the current authentication token has expired.</summary>
    bool IsExpired { get; }

    /// <summary>
    /// Authenticates the user with the provided credentials.
    /// </summary>
    /// <param name="nameid">The user's name identifier.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResult"/> indicating the outcome of the login attempt.</returns>
    Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current authenticated session.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task LogOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the current authentication token.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResult"/> indicating the outcome of the refresh attempt.</returns>
    Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <param name="newPassword">The desired new password.</param>
    /// <param name="currentPassword">The user's current password for verification.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken cancellationToken = default);
}
