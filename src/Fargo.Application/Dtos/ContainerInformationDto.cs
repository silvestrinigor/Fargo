using UnitsNet;

namespace Fargo.Application.Dtos
{
    public sealed record ContainerInformationDto(
        Mass? MassCapacity,
        Volume? VolumeCapacity,
        int? ItensQuantityCapacity,
        Temperature? DefaultTemperature
        );
}
