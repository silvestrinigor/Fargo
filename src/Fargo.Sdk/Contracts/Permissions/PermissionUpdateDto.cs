namespace Fargo.Sdk.Contracts.Permissions;

/// <summary>Represents a permission entry sent in create or update requests.</summary>
public sealed record PermissionUpdateDto(ActionType Action);

