using Fargo.Application.Shared.Identity;
using Fargo.HttpApi.Client.Common;

namespace Fargo.HttpApi.Client.Identity;

public sealed class IdentityHttpApiClient(FargoHttpClient fargoHttpClient) : IIdentityHttpApiClient
{
    public Task<FargoHttpResponse> ChangePasswordAsync(PasswordUpdateDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PutAsync("identity/password", request, cancellationToken);

    public Task<FargoHttpResponse<AuthResult>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PostAsync<LoginDto, AuthResult>("identity/login", request, cancellationToken);

    public Task<FargoHttpResponse> LogoutAsync(RefreshDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PostAsync("identity/logout", request, cancellationToken);

    public Task<FargoHttpResponse<AuthResult>> RefreshAsync(RefreshDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PostAsync<RefreshDto, AuthResult>("identity/logout", request, cancellationToken);
}
