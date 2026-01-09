using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticleDtos
{
    public record ArticleCreateDto(
        Name Name,
        Description Description
        );
}
