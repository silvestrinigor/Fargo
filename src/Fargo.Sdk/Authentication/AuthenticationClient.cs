using Fargo.Api.Contracts.Authentication;
using Fargo.Api.Http;

namespace Fargo.Api.Authentication;

public sealed class AuthenticationClient : IAuthenticationClient
{
    internal AuthenticationClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<AuthDto>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<LoginDto, AuthDto>(
            "/authentication/login",
            new LoginDto(nameid, password),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthDto>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthDto>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<AuthDto>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<RefreshDto, AuthDto>(
            "/authentication/refresh",
            new RefreshDto(refreshToken),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthDto>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthDto>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync<RefreshDto>(
            "/authentication/logout",
            new RefreshDto(refreshToken),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PasswordUpdateDto>(
            "/authentication/password",
            new PasswordUpdateDto(newPassword, currentPassword),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
