namespace Fargo.ClientAuthentication;

/// <summary>
/// The tokens issued by the Fargo API at login or refresh.
/// </summary>
/// <param name="AccessToken">The bearer token sent with requests.</param>
/// <param name="RefreshToken">
/// The single-use token that obtains new tokens. The API revokes it on use and
/// returns a new one.
/// </param>
/// <param name="ExpiresAt">The instant the access token expires.</param>
public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
