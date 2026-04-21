using Fargo.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Fargo.Infrastructure.Converters;

public sealed class LengthStringConverter()
    : ValueConverter<Length, string>(
        l => l.ToUnitsNet().ToString("G17", CultureInfo.InvariantCulture),
        s => (Length)UnitsNet.Length.Parse(s, CultureInfo.InvariantCulture));
