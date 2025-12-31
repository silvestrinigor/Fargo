using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Application.Solicitations.Commands.ArticleCommands
{
    public sealed record ArticleCreateCommand(
        Name? Name,
        Description? Description,
        TimeSpan? ShelfLife,
        Length? Length,
        Length? Width,
        Length? Height,
        Mass? Mass,
        Volume? Volume,
        Density? Density,
        ArticleCreateCommandContainerInformation ContainerInformation);

    public sealed record ArticleCreateCommandContainerInformation(
        Mass? MassCapacity,
        Volume? VolumeCapacity,
        int? ItensQuantityCapacity,
        Temperature? DefaultTemperature);
}
