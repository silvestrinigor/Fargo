using UnitsNet;

namespace Fargo.Domain.ValueObjects
{
    public class ItemRequirements
    {
        public Temperature? MaxAmbientTemperature { get; init; }

        public Temperature? MinAmbientTemperature { get; init; }

        public Pressure? MaxAmbientPressure { get; init; }

        public Pressure? MinAmbientPressure { get; init; }
    }
}
