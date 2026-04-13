using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.Converters;

public class LengthDoubleConverter()
    : ValueConverter<Length, double>(l => l.Meters, m => Length.FromMeters(m));
