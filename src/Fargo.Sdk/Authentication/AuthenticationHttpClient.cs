using Fargo.Sdk.Contracts.Authentication;
using Fargo.Api.Http;

namespace Fargo.Api.Authentication;

/// <summary>Default implementation of <see cref="IAuthenticationHttpClient"/>.</summary>
public sealed class AuthenticationHttpClient : IAuthenticationHttpClient
{
    /// <summary>Initializes a new instance.</summary>
    public AuthenticationHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
