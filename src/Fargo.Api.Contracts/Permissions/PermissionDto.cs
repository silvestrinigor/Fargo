namespace Fargo.Api.Contracts.Permissions;

/// <summary>Represents a permission returned by the API.</summary>
public sealed record PermissionDto(
    Guid Guid,
    ActionType Action);
