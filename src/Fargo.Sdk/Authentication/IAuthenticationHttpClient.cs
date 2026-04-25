namespace Fargo.Sdk.Authentication;

/// <summary>Low-level HTTP transport for authentication endpoints.</summary>
public interface IAuthenticationHttpClient
{
    Task<FargoSdkResponse<AuthResult>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<AuthResult>> Refresh(string refreshToken, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default);
}
