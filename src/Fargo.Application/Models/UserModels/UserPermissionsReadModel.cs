using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserModels
{
    public sealed record UserPermissionReadModel(
            Guid UserGuid,
            ActionType Action
            );
}