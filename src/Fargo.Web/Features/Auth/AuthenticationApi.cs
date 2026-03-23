using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Auth;

public sealed class AuthenticationApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, sessionAccessor, httpJsonOptions)
{
    public async Task<AuthResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostAsJsonAsync(
            "/authentication/login",
            command,
            requireAuthentication: false,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await ReadFromJsonAsync<AuthResult>(response.Content, cancellationToken);

        return result ?? throw new InvalidOperationException("Authentication API returned no content.");
    }

    public async Task LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostAsJsonAsync(
            "/authentication/logout",
            command,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthResult> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostAsJsonAsync(
            "/authentication/refresh",
            command,
            requireAuthentication: false,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await ReadFromJsonAsync<AuthResult>(response.Content, cancellationToken);

        return result ?? throw new InvalidOperationException("Refresh API returned no content.");
    }

    public async Task ChangePasswordAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await PutAsJsonAsync(
            "/authentication/password",
            command,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
