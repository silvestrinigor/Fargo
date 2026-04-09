using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationClient : IAuthenticationClient
{
    internal AuthenticationClient(FargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly FargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<AuthResult>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/login",
            new { nameid, password },
            cancellationToken);

        return new FargoSdkResponse<AuthResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<AuthResult>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/refresh",
            new { refreshToken },
            cancellationToken);

        return new FargoSdkResponse<AuthResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync(
            "/authentication/logout",
            new { refreshToken },
            cancellationToken);

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync(
            "/authentication/password",
            new { passwords = new { newPassword, currentPassword } },
            cancellationToken);

        return new FargoSdkResponse<EmptyResult>();
    }
}
