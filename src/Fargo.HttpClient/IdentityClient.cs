using Fargo.Application.Shared.Identity;
using Fargo.Application.Shared.Users;

namespace Fargo.HttpClient;

public interface IFargoIdentityClient
{
    Task<AuthResult> LoginAsync(
        LoginDto request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        RefreshDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(
        RefreshDto request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        UserPasswordUpdateDto request,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoIdentityClient(FargoHttpTransport transport) : IFargoIdentityClient
{
    public Task<AuthResult> LoginAsync(
        LoginDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<AuthResult>(
            HttpMethod.Post,
            "/identity/login",
            request,
            cancellationToken);

    public Task LogoutAsync(
        RefreshDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Post,
            "/identity/logout",
            request,
            cancellationToken);

    public Task<AuthResult> RefreshAsync(
        RefreshDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<AuthResult>(
            HttpMethod.Post,
            "/identity/refresh",
            request,
            cancellationToken);

    public Task ChangePasswordAsync(
        UserPasswordUpdateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            "/identity/password",
            request,
            cancellationToken);
}
