using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public record ItemDto(
        Guid Guid,
        Guid ArticleGuid,
        DateTime CreatedAt
        );

    public record ItemCreateDto(
        Guid ArticleGuid
        );
}
