namespace Fargo.ClientHttp.Authentication;

/// <summary>
/// The tokens issued by the Fargo API at login or refresh.
/// </summary>
/// <param name="RefreshToken">
/// Single-use: the API revokes it on use and returns a new one.
/// </param>
public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
