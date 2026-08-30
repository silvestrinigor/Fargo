namespace Fargo.Http.Client.Authentication;

public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
