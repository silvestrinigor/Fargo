using Fargo.Sdk.Contracts.Authentication;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Authentication;

/// <summary>Default implementation of <see cref="IAuthenticationClient"/>.</summary>
public sealed class AuthenticationHttpClient : IAuthenticationClient
{
    /// <summary>Initializes a new instance.</summary>
    public AuthenticationHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<AuthInfo>> LogInAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<LoginRequest, AuthInfo>(
            "/authentication/login",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<AuthInfo>> Refresh(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<RefreshRequest, AuthInfo>(
            "/authentication/refresh",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> LogOutAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync<RefreshRequest>(
            "/authentication/logout",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> ChangePassword(PasswordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PasswordUpdateRequest>(
            "/authentication/password",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }
}
