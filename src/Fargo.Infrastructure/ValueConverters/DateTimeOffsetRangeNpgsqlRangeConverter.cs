using Fargo.Core.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Fargo.Infrastructure.ValueConverters;

public class DateTimeOffsetRangeNpgsqlRangeConverter()
    : ValueConverter<DateTimeOffsetRange, NpgsqlRange<DateTimeOffset>>(
        value => new NpgsqlRange<DateTimeOffset>(
            value.Start, lowerBoundIsInclusive: true, value.End, upperBoundIsInclusive: false),
        value => new DateTimeOffsetRange(
            value.LowerBound, value.UpperBound)
        );
