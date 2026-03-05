using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserResponseModel(
            Nameid Nameid,
            Description Description,
            List<ActionType> Permissions
            );
}