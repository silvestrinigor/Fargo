using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticlesDtos
{
    public record ArticleCreateDto(
        Name Name,
        Description Description
        );
}
