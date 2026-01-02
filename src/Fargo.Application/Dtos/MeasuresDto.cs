using UnitsNet;

namespace Fargo.Application.Dtos
{
    public sealed record MeasuresDto(
        Length? X,
        Length? Y,
        Length? Z,
        Volume? Volume,
        Mass? Mass,
        Density? Density
        );
}
