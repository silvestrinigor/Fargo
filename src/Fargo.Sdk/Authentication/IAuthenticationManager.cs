namespace Fargo.Sdk.Authentication;

public interface IAuthenticationManager
{
    Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    event EventHandler<LoggedInEventArgs>? LoggedIn;

    Task LogOutAsync(CancellationToken cancellationToken = default);

    event EventHandler<LoggedOutEventArgs>? LoggedOut;

    Task<AuthResult> RefreshAsync(CancellationToken cancellationToken = default);

    event EventHandler<RefreshedEventArgs>? Refreshed;

    Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default);

    event EventHandler<PasswordChangedEventArgs>? PasswordChanged;
}
