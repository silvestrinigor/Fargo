using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data required to create a new user.
    /// </summary>
    /// <param name="Nameid">
    /// The unique login identifier of the user.
    /// </param>
    /// <param name="Password">
    /// The plaintext password that will be hashed before being stored.
    /// </param>
    /// <param name="Description">
    /// Optional description of the user.
    /// </param>
    /// <param name="Permissions">
    /// Optional list of permissions granted to the user.
    /// </param>
    public record UserCreateModel(
            Nameid Nameid,
            Password Password,
            Description? Description = null,
            IReadOnlyCollection<ActionType>? Permissions = null
            );
}