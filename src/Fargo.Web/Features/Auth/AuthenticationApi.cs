using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.Web.Api;

namespace Fargo.Web.Features.Auth;

public sealed class AuthenticationApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor)
    : FargoApiClientBase(httpClientFactory, sessionAccessor)
{
    public async Task<AuthResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient(requireAuthentication: false)
            .PostAsJsonAsync("/authentication/login", command, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("Authentication API returned no content.");
    }

    public async Task LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient()
            .PostAsJsonAsync("/authentication/logout", command, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthResult> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient(requireAuthentication: false)
            .PostAsJsonAsync("/authentication/refresh", command, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("Refresh API returned no content.");
    }

    public async Task ChangePasswordAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateClient()
            .PutAsJsonAsync("/authentication/password", command, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
