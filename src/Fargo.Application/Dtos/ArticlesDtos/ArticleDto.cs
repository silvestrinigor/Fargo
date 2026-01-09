using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticlesDtos
{
    public record ArticleDto(
        Guid Guid,
        Name Name,
        Description Description
        );
}
