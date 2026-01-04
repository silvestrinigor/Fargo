using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public record ArticleCreateDto(
        Name Name,
        Description Description
        );
}
