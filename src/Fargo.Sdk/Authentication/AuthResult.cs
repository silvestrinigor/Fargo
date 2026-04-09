namespace Fargo.Sdk.Authentication;

public sealed record AuthResult
(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
