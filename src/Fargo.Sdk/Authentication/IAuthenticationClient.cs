namespace Fargo.Sdk.Authentication;

/// <summary>High-level authentication client interface for managing user sessions.</summary>
public interface IAuthenticationClient
{
    /// <summary>Authenticates the user with the given credentials and returns a new session.</summary>
    /// <param name="nameid">The user's login name identifier.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<AuthDto>> LogInAsync(string nameid, string password, CancellationToken cancellationToken = default);

    /// <summary>Refreshes an access token using a valid refresh token.</summary>
    /// <param name="refreshToken">The refresh token from a previous session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<AuthDto>> Refresh(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Invalidates the session associated with the given refresh token.</summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> LogOutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Changes the authenticated user's password.</summary>
    /// <param name="newPassword">The desired new password.</param>
    /// <param name="currentPassword">The current password for verification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> ChangePassword(string newPassword, string currentPassword, CancellationToken cancellationToken = default);
}
