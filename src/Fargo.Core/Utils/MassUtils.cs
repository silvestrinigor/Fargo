using UnitsNet;

namespace Fargo.Domain.Utils
{
    public static class MassUtils
    {
        public static Mass Convert(double value, Enuns.MassUnit unit)
        {
            switch (unit)
            {
                case Enuns.MassUnit.Gram:
                    return Mass.FromGrams(value);
                case Enuns.MassUnit.Kilogram:
                    return Mass.FromKilograms(value);
                default:
                    throw new NotSupportedException($"The mass unit '{unit}' is not supported.");
            }
        }

        public static double Convert(Mass mass, Enuns.MassUnit unit)
        {
            switch (unit)
            {
                case Enuns.MassUnit.Gram:
                    return mass.Grams;
                case Enuns.MassUnit.Kilogram:
                    return mass.Kilograms;
                default:
                    throw new NotSupportedException($"The mass unit '{unit}' is not supported.");
            }
        }
    }
}