using UnitsNet;

namespace Fargo.Domain.ValueObjects
{
    public class ItemPropeties
    {
        public Mass? Mass { get; init; }

        public Length? LengthX { get; init; }

        public Length? LengthY { get; init; }

        public Length? LengthZ { get; init; }

        public Volume? Volume
            => LengthX.HasValue && LengthY.HasValue && LengthZ.HasValue
            ? LengthX.Value * LengthY.Value * LengthZ.Value
            : null;

        public Density? Density
            => Mass.HasValue && Volume.HasValue
            ? Mass.Value / Volume.Value
            : null;

        public ItemPropeties(Volume space)
        {
            VolumeSpace = space;
        }

        public ItemPropeties(Length x, Length y, Length z)
        {
            LengthX = x;
            LengthY = y;
            LengthZ = z;

            VolumeSpace = x * y * z;
        }
    }
}
