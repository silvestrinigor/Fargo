using System.ComponentModel.DataAnnotations;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// Configuration of the RabbitMQ event publisher, bound from the
/// <c>RabbitMqEvents</c> section. The broker connection itself is configured by
/// the Aspire RabbitMQ client integration.
/// </summary>
public sealed class RabbitMqEventOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "RabbitMqEvents";

    /// <summary>
    /// Gets or sets the durable topic exchange events are published to.
    /// Defaults to <c>fargo.events</c>.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string ExchangeName { get; set; } = "fargo.events";

    /// <summary>
    /// Gets or sets how long a publish may wait for the broker's confirm before
    /// it fails. Defaults to ten seconds.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:10:00")]
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
