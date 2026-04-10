namespace Fargo.Sdk.Authentication;

public sealed record StoredSession(
    string Nameid,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
