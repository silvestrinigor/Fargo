namespace Fargo.Sdk.Contracts.Permissions;

/// <summary>Represents a permission returned by the API.</summary>
public sealed record PermissionInfo(
    Guid Guid,
    ActionType Action);
