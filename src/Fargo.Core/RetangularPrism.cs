using UnitsNet;

namespace Fargo.Domain
{
    public class RetangularPrism
    {
        public Length X { get; set; }

        public Length Y { get; set; }

        public Length Z { get; set; }

        public Volume Volume => X * Y * Z;

        public Area TotalSurfaceArea => 2 * ( (X * Y) + (X * Z) + (Y * Z));
    }
}
