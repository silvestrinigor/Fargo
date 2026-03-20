using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class AuthenticationClient(HttpClient http)
    : FargoHttpClientBase(http), IAuthenticationClient
{
    public Task<AuthResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthResult>(
            "/authentication/login",
            command,
            cancellationToken);
    }

    public Task LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("/authentication/logout", command, cancellationToken);
    }

    public Task<AuthResult> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AuthResult>(
            "/authentication/refresh",
            command,
            cancellationToken);
    }

    public Task ChangePasswordAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default)
    {
        return PutAsync("/authentication/password", command, cancellationToken);
    }
}
