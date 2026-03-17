using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels;

/// <summary>
/// Represents the data required to assign a permission to a user.
/// </summary>
/// <param name="Action">
/// The action that the user is permitted to perform.
/// </param>
public sealed record UserPermissionUpdateModel(
        ActionType Action
        );
