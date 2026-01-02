using UnitsNet;

namespace Fargo.Domain.Extensions
{
    public static class NotNegativeUnitsNetExtension
    {
        extension<T>(T quantity) where T : IQuantity
        {
            public T NotNegative()
            {
                if (quantity.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot be negative.");
                }

                return quantity;
            }
        }
    }
}
