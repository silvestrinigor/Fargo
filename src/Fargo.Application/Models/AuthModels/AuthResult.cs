using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.AuthModels
{
    /// <summary>
    /// Represents the result of a successful authentication.
    /// </summary>
    /// <param name="AccessToken">
    /// The generated access token used to authenticate API requests.
    /// </param>
    /// <param name="RefreshToken">
    /// The refresh token used to obtain a new access token when the current one expires.
    /// </param>
    /// <param name="ExpiresAt">
    /// The date and time when the access token expires.
    /// </param>
    public record AuthResult(
            Token AccessToken,
            Token RefreshToken,
            DateTimeOffset ExpiresAt
            );
}