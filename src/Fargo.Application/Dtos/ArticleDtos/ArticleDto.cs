using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticleDtos
{
    public record ArticleDto(
        Guid Guid,
        Name Name,
        Description Description,
        bool IsContainer);
}
