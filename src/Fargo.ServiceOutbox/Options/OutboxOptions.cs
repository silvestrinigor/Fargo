using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceOutbox.Options;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(typeof(TimeSpan), "00:00:00.010", "01:00:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    [Range(1, 10_000)]
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// The wait after consecutive failures doubles from <see cref="PollInterval"/> up to this value.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "01:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(1);

    [Range(typeof(TimeSpan), "00:01:00", "3650.00:00:00")]
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromHours(1);
}
