using Fargo.Sdk.Contracts.Authentication;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

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
    public async Task<FargoSdkResponse<AuthInfo>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<LoginRequest, AuthInfo>(
            "/authentication/login",
            new LoginRequest(nameid, password),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthInfo>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<AuthInfo>> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<RefreshRequest, AuthInfo>(
            "/authentication/refresh",
            new RefreshRequest(refreshToken),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthInfo>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<AuthInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync<RefreshRequest>(
            "/authentication/logout",
            new RefreshRequest(refreshToken),
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
        var httpResponse = await httpClient.PutJsonAsync<PasswordUpdateRequest>(
            "/authentication/password",
            new PasswordUpdateRequest(newPassword, currentPassword),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
