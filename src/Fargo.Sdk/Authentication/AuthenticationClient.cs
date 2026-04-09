using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationClient : IAuthenticationClient
{
    internal AuthenticationClient(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly FargoHttpClient httpClient;

    public Task<AuthResult> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        return httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/login",
            new { nameid, password },
            cancellationToken);
    }

    public Task<AuthResult> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        return httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/refresh",
            new { refreshToken },
            cancellationToken);
    }

    public Task LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return httpClient.PostJsonAsync(
            "/authentication/logout",
            new { refreshToken });
    }

    public Task ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        return httpClient.PutJsonAsync(
            "/authentication/password",
            new { passwords = new { newPassword, currentPassword } });
    }
}
