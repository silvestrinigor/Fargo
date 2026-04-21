using Fargo.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Fargo.Infrastructure.Converters;

public sealed class MassStringConverter()
    : ValueConverter<Mass, string>(
        m => m.ToUnitsNet().ToString("G17", CultureInfo.InvariantCulture),
        s => (Mass)UnitsNet.Mass.Parse(s, CultureInfo.InvariantCulture));
