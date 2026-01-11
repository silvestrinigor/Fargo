using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels
{
    public sealed record PermissionReadModel(
        Guid UserGuid,
        ActionType ActionType,
        GrantType GrantType);
}
