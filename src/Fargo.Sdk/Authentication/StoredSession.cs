namespace Fargo.Sdk.Authentication;

public sealed record StoredSession(
    string Nameid,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
