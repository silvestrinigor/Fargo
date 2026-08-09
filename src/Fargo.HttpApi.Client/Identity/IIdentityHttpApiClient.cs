using Fargo.Application.Shared.Identity;

namespace Fargo.HttpApi.Client.Identity;

public interface IIdentityHttpApiClient
{
    Task<AuthResult> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);

    Task LogoutAsync(RefreshDto request, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(RefreshDto request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(PasswordUpdateDto request, CancellationToken cancellationToken = default);
}
