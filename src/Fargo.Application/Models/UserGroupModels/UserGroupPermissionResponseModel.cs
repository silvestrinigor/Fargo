using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserGroupModels
{
    /// <summary>
    /// Represents the response model of a permission assigned to a user group.
    /// </summary>
    /// <param name="Guid">
    /// The unique identifier of the permission entry.
    /// </param>
    /// <param name="Action">
    /// The action granted to the user group.
    /// </param>
    public sealed record UserGroupPermissionResponseModel(
        Guid Guid,
        ActionType Action
    );
}