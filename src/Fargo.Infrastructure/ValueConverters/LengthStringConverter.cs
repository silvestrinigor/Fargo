using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;
using UnitsNet;

namespace Fargo.Infrastructure.Converters;

public sealed class LengthStringConverter()
    : ValueConverter<Length, string>(
        l => l.ToString("G17", CultureInfo.InvariantCulture),
        s => Length.Parse(s, CultureInfo.InvariantCulture));
