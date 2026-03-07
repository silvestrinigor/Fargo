using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the user data returned by the application.
    /// </summary>
    /// <param name="Nameid">
    /// The login identifier of the user.
    /// </param>
    /// <param name="Description">
    /// The description associated with the user.
    /// </param>
    /// <param name="Permissions">
    /// The permissions granted to the user.
    /// </param>
    public sealed record UserResponseModel(
            Nameid Nameid,
            Description Description,
            IReadOnlyCollection<ActionType> Permissions
            );
}