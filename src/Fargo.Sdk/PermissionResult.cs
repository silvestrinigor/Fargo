namespace Fargo.Api;

/// <summary>
/// Represents a permission entry returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the permission entry.</param>
/// <param name="Action">The action this permission grants.</param>
public sealed record PermissionResult(
    Guid Guid,
    ActionType Action
);
