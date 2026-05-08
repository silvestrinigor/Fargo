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
    public async Task<FargoSdkResponse<AuthInfo>> LogInAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<LoginRequest, AuthInfo>(
            "/authentication/login",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<AuthInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<AuthInfo>> Refresh(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<RefreshRequest, AuthInfo>(
            "/authentication/refresh",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<AuthInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<AuthInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> LogOutAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync<RefreshRequest>(
            "/authentication/logout",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> ChangePassword(PasswordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PasswordUpdateRequest>(
            "/authentication/password",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError<T>(FargoSdkHttpResponse<T> response) => FargoSdkProblemMapper.Map(response.Problem, response.StatusCode);
}
