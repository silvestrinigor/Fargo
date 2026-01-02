using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Application.Dtos
{
    public record ArticleDto(
        Guid Guid,
        Name? Name,
        Description? Description,
        TimeSpan? ShelfLife,
        Temperature? MaximumContainerTemperature,
        Temperature? MinimumContainerTemperature,
        MeasuresDto Measures,
        ContainerInformationDto? Container
        );

    public record ArticleCreateDto(
        Name? Name,
        Description? Description,
        TimeSpan ShelfLife,
        Temperature? MaxContainerTemperature,
        Temperature? MinContainerTemperature,
        MeasuresDto Measures,
        ContainerInformationDto? Container
        );
}
