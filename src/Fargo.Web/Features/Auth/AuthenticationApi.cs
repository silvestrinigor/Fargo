using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.HttpApi.Client.Contracts;

namespace Fargo.Web.Features.Auth;

public sealed class AuthenticationApi(IAuthenticationClient authenticationClient)
{
    public Task<AuthResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default) =>
        authenticationClient.LoginAsync(command, cancellationToken);
}
