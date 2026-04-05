namespace Fargo.Sdk.Authentication;

public interface IAuthenticationClient
{
    Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    Task<AuthResult> Refresh(string refreshToken, CancellationToken cancellationToken = default);

    Task LogOutAsync(string refreshToken);

    Task ChangePassword(string newPassword, string currentPassword);
}
