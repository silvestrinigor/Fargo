using Fargo.Application.Models.AuthModels;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class AuthenticationClient(HttpClient http)
    : FargoHttpClientBase(http), IAuthenticationClient
{
    public Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
        => PostAsync<AuthResult>("/authentication/login", command, ct);

    public Task LogoutAsync(LogoutCommand command, CancellationToken ct = default)
        => PostAsync("/authentication/logout", command, ct);

    public Task<AuthResult> RefreshAsync(RefreshCommand command, CancellationToken ct = default)
        => PostAsync<AuthResult>("/authentication/refresh", command, ct);

    public Task ChangePasswordAsync(PasswordChangeCommand command, CancellationToken ct = default)
        => PostAsync("/authentication/password", command, ct);
}