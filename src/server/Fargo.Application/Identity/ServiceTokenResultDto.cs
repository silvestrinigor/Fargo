namespace Fargo.Application.Identity;

/// <summary>
/// Result of a successful service-account (client-credentials) authentication.
/// </summary>
/// <remarks>
/// Unlike a user login, a service authentication returns no refresh token: a
/// service simply re-exchanges its client credentials for a new access token
/// when the current one nears expiry.
/// </remarks>
/// <param name="AccessToken">The generated access token used to authenticate API requests.</param>
/// <param name="ExpiresAt">The date and time when the access token expires.</param>
public record ServiceTokenResultDto(
    string AccessToken,
    DateTimeOffset ExpiresAt
);
