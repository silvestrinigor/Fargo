namespace Fargo.Sdk.Authentication;

public interface IAuthSession
{
    string? Nameid { get; }

    string? AccessToken { get; }

    string? RefreshToken { get; }

    DateTimeOffset? ExpiresAt { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    IReadOnlyCollection<ActionType> PermissionActions { get; }

    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    bool HasActionPermission(ActionType action);

    bool HasPartitionAccess(Guid partitionGuid);
}
