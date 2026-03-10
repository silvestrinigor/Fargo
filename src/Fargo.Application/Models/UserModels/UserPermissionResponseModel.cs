using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the permission data returned by the application.
    /// </summary>
    /// <param name="Guid">
    /// The guid of the permission.
    /// </param>
    /// <param name="Action">
    /// The action that the user is allowed to perform.
    /// </param>
    public sealed record UserPermissionResponseModel(
            Guid Guid,
            ActionType Action
            );
}