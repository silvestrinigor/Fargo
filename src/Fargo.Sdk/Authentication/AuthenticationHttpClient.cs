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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    private static FargoSdkError MapError(FargoProblemDetails? problem)
    {
        var type = problem?.Type switch
        {
            "auth/unauthorized" => FargoSdkErrorType.UnauthorizedAccess,
            "auth/invalid-credentials" => FargoSdkErrorType.InvalidCredentials,
            "auth/invalid-password" => FargoSdkErrorType.InvalidCredentials,
            "auth/password-change-required" => FargoSdkErrorType.PasswordChangeRequired,
            _ => FargoSdkErrorType.Undefined
        };

        var detail = problem?.Detail ?? "An unexpected error occurred.";

        return new FargoSdkError(type, detail);
    }
}
