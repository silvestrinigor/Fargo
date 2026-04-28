using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;
using UnitsNet;

namespace Fargo.Infrastructure.Converters;

public sealed class MassStringConverter()
    : ValueConverter<Mass, string>(
        m => m.ToString("G17", CultureInfo.InvariantCulture),
        s => Mass.Parse(s, CultureInfo.InvariantCulture));
