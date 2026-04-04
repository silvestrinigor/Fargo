using Fargo.Sdk.Http;
using Fargo.Sdk.Models;
using Fargo.Sdk.Security;

namespace Fargo.Sdk.Services;

public sealed class AuthService
{
    private readonly FargoHttpClient httpClient;
    private readonly FargoAuthSession session;

    public AuthService(FargoHttpClient httpClient, FargoAuthSession session)
    {
        this.httpClient = httpClient;
        this.session = session;
    }

    public async Task<AuthResult> LoginAsync(string nameid, string password, CancellationToken ct = default)
    {
        var result = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/login",
            new { nameid, password },
            ct);

        session.SetTokens(result.AccessToken, result.RefreshToken, result.ExpiresAt);

        return result;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        if (!session.IsAuthenticated)
        {
            return;
        }

        var refreshToken = session.RefreshToken;

        session.Clear();

        await httpClient.PostJsonAsync(
            "/authentication/logout",
            new { refreshToken },
            ct);
    }

    public async Task<AuthResult> RefreshAsync(CancellationToken ct = default)
    {
        var result = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/refresh",
            new { refreshToken = session.RefreshToken },
            ct);

        session.SetTokens(result.AccessToken, result.RefreshToken, result.ExpiresAt);

        return result;
    }

    public Task ChangePasswordAsync(string newPassword, string currentPassword, CancellationToken ct = default)
    {
        return httpClient.PutJsonAsync(
            "/authentication/password",
            new { passwords = new { newPassword, currentPassword } },
            ct);
    }
}
