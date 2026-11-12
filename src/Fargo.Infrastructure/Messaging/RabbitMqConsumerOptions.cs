using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// Configuration of the RabbitMQ event consumer, bound from the
/// <c>RabbitMqConsumer</c> section. The exchange is the one in
/// <see cref="RabbitMqEventOptions.ExchangeName"/>.
/// </summary>
public sealed class RabbitMqConsumerOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "RabbitMqConsumer";

    /// <summary>
    /// Gets or sets how many unacknowledged messages the broker delivers ahead.
    /// Defaults to 50.
    /// </summary>
    [Range(1, 1000)]
    public ushort PrefetchCount { get; set; } = 50;

    /// <summary>
    /// Gets or sets the wait after the first failure to connect or subscribe; it
    /// doubles on each failure in a row, up to <see cref="MaxRetryDelay"/>.
    /// Defaults to one second.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:10:00")]
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the longest wait between attempts to connect or subscribe.
    /// Defaults to thirty seconds.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "01:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}
