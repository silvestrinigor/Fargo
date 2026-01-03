using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public record ArticleDto(
        Guid Guid,
        Name Name,
        Description? Description,
        DateTime CreatedAt
        );

    public record ArticleCreateDto(
        Name Name,
        Description Description
        );
}
