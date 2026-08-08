using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fargo.Infrastructure.ValueConverters;

public class TimeSpanTicksConverter()
    : ValueConverter<TimeSpan, long>(x => x.Ticks, x => TimeSpan.FromTicks(x));
