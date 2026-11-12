namespace Fargo.Application.Dtos.Identity;

/// <summary>
/// Unlike a user login, no refresh token is returned: the service re-exchanges its client
/// credentials when the access token nears expiry.
/// </summary>
public record ServiceTokenResultDto(
    string AccessToken,
    DateTimeOffset ExpiresAt
);
