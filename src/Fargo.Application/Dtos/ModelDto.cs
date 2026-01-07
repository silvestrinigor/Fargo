using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos
{
    public sealed record ModelDto(
        Guid Guid,
        ModelType ModelType
        );
}
