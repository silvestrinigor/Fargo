using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public record ItemDto(
        Guid Guid,
        Name? Name,
        Description? Description
        );

    public record ItemCreateDto(
        Name? Name,
        Description? Description,
        Guid ArticleGuid,
        DateTime? ManufacturedAt
        );
}
