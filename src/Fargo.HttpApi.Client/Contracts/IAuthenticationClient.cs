using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;

namespace Fargo.HttpApi.Client.Contracts;

public interface IAuthenticationClient
{
    Task<AuthResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        PasswordChangeCommand command,
        CancellationToken cancellationToken = default);
}
