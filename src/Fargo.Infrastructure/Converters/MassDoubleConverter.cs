using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class MassDoubleConverter()
    : ValueConverter<Mass, double>(m => m.Grams, g => Mass.FromGrams(g));
