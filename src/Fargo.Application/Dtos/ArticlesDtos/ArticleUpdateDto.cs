using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.ArticlesDtos
{
    public sealed record ArticleUpdateDto(
        Name? Name,
        Description? Description
        );
}
