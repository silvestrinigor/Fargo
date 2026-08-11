namespace Fargo.Core.Common;

public readonly struct DateTimeOffsetRange : IComparable<DateTimeOffsetRange>
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

    public int CompareTo(DateTimeOffsetRange other)
    {
        var startComparison = Start.CompareTo(other.Start);

        return startComparison != 0 ? startComparison : End.CompareTo(other.End);
    }
}
