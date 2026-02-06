using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels
{
    public sealed record PermissionUpdateModel(
            ActionType ActionType,
            GrantType GrantType
            );
}