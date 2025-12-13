using UnitsNet;

namespace Fargo.Domain.Utils
{
    public static class LengthUtils
    {
        public static Length Convert(double value, Enuns.LengthUnit unit)
        {
            switch (unit)
            {
                case Enuns.LengthUnit.Meter:
                    return Length.FromMeters(value);
                case Enuns.LengthUnit.Centimeter:
                    return Length.FromCentimeters(value);
                default:
                    throw new NotSupportedException($"The length unit '{unit}' is not supported.");
            }
        }

        public static double Convert(Length length, Enuns.LengthUnit unit)
        {
            switch (unit)
            {
                case Enuns.LengthUnit.Meter:
                    return length.Meters;
                case Enuns.LengthUnit.Centimeter:
                    return length.Centimeters;
                default:
                    throw new NotSupportedException($"The length unit '{unit}' is not supported.");
            }
        }
    }
}