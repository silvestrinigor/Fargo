using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Drawing;

namespace Fargo.Infrastructure.Converters;

public class ColorArgbConverter()
    : ValueConverter<Color, int>(x => x.ToArgb(), x => Color.FromArgb(x));
