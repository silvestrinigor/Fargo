namespace Fargo.Core.Common;

public readonly struct DateTimeOffsetRange
{
    public DateTimeOffset Start { get; }

    public DateTimeOffset End { get; }

    public DateTimeOffsetRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End date time must be after start date time.");
        }

        Start = start;
        End = end;
    }
}
