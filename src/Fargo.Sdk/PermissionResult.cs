namespace Fargo.Sdk;

/// <summary>
/// Represents a permission entry returned by the API.
/// </summary>
public sealed record PermissionResult(
    Guid Guid,
    ActionType Action
);
