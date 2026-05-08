using Fargo.Sdk.Contracts.Authentication;

namespace Fargo.Sdk.Authentication;

/// <summary>Low-level HTTP transport for authentication endpoints.</summary>
public interface IAuthenticationHttpClient
{
    /// <summary>Authenticates the user with the given credentials and returns a new session.</summary>
    /// <param name="request">The login request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<AuthInfo>> LogInAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Refreshes an access token using a valid refresh token.</summary>
    /// <param name="request">The refresh request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<AuthInfo>> Refresh(RefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>Invalidates the session associated with the given refresh token.</summary>
    /// <param name="request">The logout request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> LogOutAsync(RefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>Changes the authenticated user's password.</summary>
    /// <param name="request">The password update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> ChangePassword(PasswordUpdateRequest request, CancellationToken cancellationToken = default);
}
