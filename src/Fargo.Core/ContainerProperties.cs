using UnitsNet;

namespace Fargo.Domain
{
    public class ContainerProperties
    {
        public Length? LengthX { get; }

        public Length? LengthY { get; }

        public Length? LengthZ { get; }

        public Volume? VolumeSpace { get; }

        public ContainerProperties() { }

        public ContainerProperties(Volume space)
        {
            VolumeSpace = space;
        }

        public ContainerProperties(Length x, Length y, Length z)
        {
            LengthX = x;
            LengthY = y;
            LengthZ = z;

            VolumeSpace = x * y * z;
        }
    }
}
