using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos
{
    public sealed record ArticleUpdateDto(
        Name? Name,
        Description? Description
        );
}
