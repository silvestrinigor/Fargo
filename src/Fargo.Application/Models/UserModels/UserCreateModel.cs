using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public record UserCreateModel(
            Nameid Nameid,
            Password Password,
            Description? Description = null,
            List<ActionType>? Permissions = null
            );
}