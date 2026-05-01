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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
