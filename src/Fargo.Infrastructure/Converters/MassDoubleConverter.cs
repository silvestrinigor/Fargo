using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnitsNet;

namespace Fargo.Infrastructure.Converters;

public class MassDoubleConverter()
    : ValueConverter<Mass, double>(m => m.Grams, g => Mass.FromGrams(g));
