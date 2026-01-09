using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticleDtos
{
    public sealed record ArticleUpdateDto(
        Name? Name,
        Description? Description
        );
}
