using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos
{
    public sealed record UserPermissionDto(
        ActionType ActionType,
        GrantType GrantType
        );
}
