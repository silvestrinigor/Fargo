using Fargo.Application.Models.AuthModels;
using Fargo.Application.Requests.Commands.AuthCommands;

namespace Fargo.HttpApi.Client.Contracts
{
    /// <summary>
    /// Defines the contract for authentication-related HTTP API operations.
    /// </summary>
    public interface IAuthenticationClient
    {
        /// <summary>
        /// Authenticates a user and returns the access and refresh tokens.
        /// </summary>
        Task<AuthResult> LoginAsync(
            LoginCommand command,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        Task LogoutAsync(
            LogoutCommand command,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the access token using a refresh token.
        /// </summary>
        Task<AuthResult> RefreshAsync(
            RefreshCommand command,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Changes the password of the authenticated user.
        /// </summary>
        Task ChangePasswordAsync(
            PasswordChangeCommand command,
            CancellationToken cancellationToken = default);
    }
}