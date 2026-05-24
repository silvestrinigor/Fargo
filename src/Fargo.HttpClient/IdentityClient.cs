using Fargo.HttpContracts;

namespace Fargo.HttpClient;

public interface IFargoIdentityClient
{
    Task<AuthDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthDto> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        PasswordUpdateRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoIdentityClient(FargoHttpTransport transport) : IFargoIdentityClient
{
    public Task<AuthDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<AuthDto>(
            HttpMethod.Post,
            "/identity/login",
            request,
            cancellationToken);

    public Task LogoutAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Post,
            "/identity/logout",
            request,
            cancellationToken);

    public Task<AuthDto> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<AuthDto>(
            HttpMethod.Post,
            "/identity/refresh",
            request,
            cancellationToken);

    public Task ChangePasswordAsync(
        PasswordUpdateRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            "/identity/password",
            request,
            cancellationToken);
}
