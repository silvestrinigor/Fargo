using Fargo.Sdk.Contracts.Authentication;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

public sealed class AuthenticationClient : IAuthenticationClient
{
    internal AuthenticationClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<AuthDto>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<LoginRequest, AuthDto>(
            "/authentication/login",
            new LoginRequest(nameid, password),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthDto>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthDto>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<AuthDto>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<RefreshRequest, AuthDto>(
            "/authentication/refresh",
            new RefreshRequest(refreshToken),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthDto>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthDto>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync<LogoutRequest>(
            "/authentication/logout",
            new LogoutRequest(refreshToken),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PasswordChangeRequest>(
            "/authentication/password",
            new PasswordChangeRequest(new PasswordUpdateRequest(newPassword, currentPassword)),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
