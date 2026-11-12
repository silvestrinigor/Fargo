using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// The broker connection itself is configured by the Aspire RabbitMQ client integration.
/// </summary>
public sealed class RabbitMqEventOptions
{
    public const string SectionName = "RabbitMqEvents";

    /// <summary>
    /// Durable topic exchange events are published to.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string ExchangeName { get; set; } = "fargo.events";

    /// <summary>
    /// How long a publish waits for the broker confirm before it fails.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:10:00")]
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
