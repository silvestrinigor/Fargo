using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos.UserDtos
{
    public sealed record UserPermissionDto(
        ActionType ActionType,
        GrantType GrantType);
}
