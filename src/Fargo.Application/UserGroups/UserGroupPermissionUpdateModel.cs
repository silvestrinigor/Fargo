using Fargo.Domain;

namespace Fargo.Application.Models.UserGroupModels;

/// <summary>
/// Represents a permission update operation for a user group.
/// </summary>
/// <remarks>
/// This model defines a permission that should be assigned to
/// or maintained within a user group during update operations.
/// </remarks>
/// <param name="Action">
/// The action that the user group is allowed to perform.
/// The value corresponds to a permission defined in <see cref="ActionType"/>.
/// </param>
public sealed record UserGroupPermissionUpdateModel(
        ActionType Action
        );
