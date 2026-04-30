using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationClient : IAuthenticationClient
{
    internal AuthenticationClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<AuthResult>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/login",
            new { nameid, password },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<AuthResult>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, AuthResult>(
            "/authentication/refresh",
            new { refreshToken },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync(
            "/authentication/logout",
            new { refreshToken },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync(
            "/authentication/password",
            new { passwords = new { newPassword, currentPassword } },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
