using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Application.Dtos.EntitiesDtos
{
    public sealed record ArticleDto(
        Name? Name,
        Description? Description,
        TimeSpan? ShelfLife,
        Length? Length,
        Length? Width,
        Length? Height,
        Mass? Mass,
        Volume? Volume,
        Density? Density
        );
}
