namespace Fargo.Sdk.Authentication;

public sealed class AuthSession : IAuthSession
{
    private sealed record Snapshot(
        string Nameid,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt,
        bool IsAdmin,
        IReadOnlyCollection<ActionType> PermissionActions,
        IReadOnlyCollection<Guid> PartitionAccesses);

    private volatile Snapshot? snapshot;

    public string? Nameid => snapshot?.Nameid;

    public string? AccessToken => snapshot?.AccessToken;

    public string? RefreshToken => snapshot?.RefreshToken;

    public DateTimeOffset? ExpiresAt => snapshot?.ExpiresAt;

    public bool IsAuthenticated => snapshot is not null;

    public bool IsExpired => snapshot?.ExpiresAt < DateTimeOffset.UtcNow;

    public bool IsAdmin => snapshot?.IsAdmin ?? false;

    public IReadOnlyCollection<ActionType> PermissionActions => snapshot?.PermissionActions ?? [];

    public IReadOnlyCollection<Guid> PartitionAccesses => snapshot?.PartitionAccesses ?? [];

    public bool HasActionPermission(ActionType action)
        => IsAdmin || PermissionActions.Contains(action);

    public bool HasPartitionAccess(Guid partitionGuid)
        => IsAdmin || PartitionAccesses.Contains(partitionGuid);

    internal void SetTokens(
        string nameid,
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        bool isAdmin,
        IReadOnlyCollection<ActionType> permissionActions,
        IReadOnlyCollection<Guid> partitionAccesses)
        => snapshot = new Snapshot(nameid, accessToken, refreshToken, expiresAt, isAdmin, permissionActions, partitionAccesses);

    internal void Clear() => snapshot = null;
}
