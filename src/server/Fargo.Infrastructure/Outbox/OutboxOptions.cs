using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// Configuration of the outbox relay, bound from the <c>Outbox</c> section.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "Outbox";

    /// <summary>
    /// Gets or sets how long the relay waits before checking again once the
    /// outbox is empty. Defaults to one second.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "01:00:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum number of messages claimed per batch. Defaults
    /// to 100.
    /// </summary>
    [Range(1, 10_000)]
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the longest wait between retries after consecutive
    /// failures. The wait doubles from <see cref="PollInterval"/> up to this
    /// value. Defaults to one minute.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.010", "01:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets how long published messages are kept before they are
    /// deleted. Defaults to seven days.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "3650.00:00:00")]
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets how often published messages past their retention are
    /// deleted. Defaults to one hour.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromHours(1);
}
