using UnitsNet;

namespace Fargo.Domain.Extensions
{
    public static class LengthExtensions
    {
        public static Length Convert(double value, Enuns.LengthUnit unit)
        {
            return unit switch
            {
                Enuns.LengthUnit.Meter => Length.FromMeters(value),
                Enuns.LengthUnit.Centimeter => Length.FromCentimeters(value),
                _ => throw new NotSupportedException($"The length unit '{unit}' is not supported."),
            };
        }

        public static double Convert(Length length, Enuns.LengthUnit unit)
        {
            return unit switch
            {
                Enuns.LengthUnit.Meter => length.Meters,
                Enuns.LengthUnit.Centimeter => length.Centimeters,
                _ => throw new NotSupportedException($"The length unit '{unit}' is not supported."),
            };
        }
    }
}