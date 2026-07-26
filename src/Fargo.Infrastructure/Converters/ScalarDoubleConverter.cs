using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnitsNet;

namespace Fargo.Infrastructure.Converters;

public class ScalarDoubleConverter()
    : ValueConverter<Scalar, double>(x => x.Amount, x => Scalar.FromAmount(x));
