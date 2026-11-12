using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// The exchange is the one in <see cref="RabbitMqEventOptions.ExchangeName"/>.
/// </summary>
public sealed class RabbitMqConsumerOptions
{
    public const string SectionName = "RabbitMqConsumer";

    /// <summary>
    /// Unacknowledged messages the broker delivers ahead.
    /// </summary>
    [Range(1, 1000)]
    public ushort PrefetchCount { get; set; } = 50;

    /// <summary>
    /// Doubles on each consecutive failure, up to <see cref="MaxRetryDelay"/>.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:10:00")]
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    [Range(typeof(TimeSpan), "00:00:00.100", "01:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}
