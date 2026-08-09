using Fargo.Application.Shared.Identity;

namespace Fargo.HttpApi.Client.Identity;

public sealed class IdentityHttpApiClient : IIdentityHttpApiClient
{
    public Task ChangePasswordAsync(PasswordUpdateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResult> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(RefreshDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResult> RefreshAsync(RefreshDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
