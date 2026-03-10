using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents a permission assigned to a user in read operations.
    /// </summary>
    /// <param name="Guid">
    /// The guid of the permission.
    /// </param>
    /// <param name="UserGuid">
    /// The unique identifier of the user who owns the permission.
    /// </param>
    /// <param name="Action">
    /// The action that the user is allowed to perform.
    /// </param>
    public sealed record UserPermissionReadModel(
            Guid Guid,
            Guid UserGuid,
            ActionType Action
            );
}