using UnitsNet;

namespace Fargo.Domain.Extensions
{
    public static class MassExtensions
    {
        extension(double value)
        {
            public Mass ToMass(Enuns.MassUnit unit)
            {
                return unit switch
                {
                    Enuns.MassUnit.Gram => Mass.FromGrams(value),
                    Enuns.MassUnit.Kilogram => Mass.FromKilograms(value),
                    _ => throw new NotSupportedException($"The mass unit '{unit}' is not supported."),
                };
            }
        }

        extension(Mass mass)
        {
            public double ToUnit(Enuns.MassUnit unit)
            {
                return unit switch
                {
                    Enuns.MassUnit.Gram => mass.Grams,
                    Enuns.MassUnit.Kilogram => mass.Kilograms,
                    _ => throw new NotSupportedException($"The mass unit '{unit}' is not supported."),
                };
            }
        }
    }
}