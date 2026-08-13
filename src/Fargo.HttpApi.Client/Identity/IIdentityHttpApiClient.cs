using Fargo.Application.Shared.Identity;
using Fargo.HttpApi.Client.Common;

namespace Fargo.HttpApi.Client.Identity;

public interface IIdentityHttpApiClient
{
    Task<FargoHttpResponse<AuthResult>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse> LogoutAsync(RefreshDto request, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse<AuthResult>> RefreshAsync(RefreshDto request, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse> ChangePasswordAsync(PasswordUpdateDto request, CancellationToken cancellationToken = default);
}
