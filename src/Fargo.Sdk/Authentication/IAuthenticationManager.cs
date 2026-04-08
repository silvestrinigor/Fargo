namespace Fargo.Sdk.Authentication;

public interface IAuthenticationManager
{
    event EventHandler<LoggedInEventArgs>? LoggedIn;

    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    event EventHandler<RefreshedEventArgs>? Refreshed;

    event EventHandler<PasswordChangedEventArgs>? PasswordChanged;

    Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    Task LogOutAsync(CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default);

    Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default);
}
