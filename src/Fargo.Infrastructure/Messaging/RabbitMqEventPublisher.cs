using Fargo.Application.IntegrationEvents;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// Publishes outbound events to a RabbitMQ topic exchange, with publisher
/// confirms.
/// </summary>
/// <remarks>
/// <para>
/// Each event is published to <see cref="RabbitMqEventOptions.ExchangeName"/>
/// with its event type as the routing key (for example <c>article.created</c>),
/// as a persistent message whose message id is the event id. A publish returns
/// only once the broker confirms it, and throws otherwise.
/// </para>
/// <para>
/// The connection is resolved on the first publish, not at construction, so the
/// application starts while the broker is unavailable; the outbox keeps the
/// events until it is back. The channel is recreated after a failure. Messages
/// with no bound queue are dropped by the broker, as usual for a topic exchange.
/// </para>
/// </remarks>
public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    public const string SchemaVersionHeader = "schema-version";

    private readonly Func<IConnection> connectionFactory;
    private readonly RabbitMqEventOptions options;
    private readonly SemaphoreSlim gate = new(1, 1);
    private IChannel? channel;

    public RabbitMqEventPublisher(Func<IConnection> connectionFactory, IOptions<RabbitMqEventOptions> options)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(options);

        this.connectionFactory = connectionFactory;
        this.options = options.Value;
    }

    public async Task PublishAsync(OutboundEvent outboundEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboundEvent);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.PublishTimeout);

        await gate.WaitAsync(cancellationToken);

        try
        {
            var openChannel = await GetChannelAsync(timeout.Token);

            try
            {
                await openChannel.BasicPublishAsync(
                    options.ExchangeName,
                    outboundEvent.EventType,
                    mandatory: false,
                    CreateProperties(outboundEvent),
                    Encoding.UTF8.GetBytes(outboundEvent.Body),
                    timeout.Token);
            }
            catch
            {
                // The channel state is unknown after a failed or timed-out
                // publish; start from a fresh channel next time.
                await CloseChannelAsync();

                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public static BasicProperties CreateProperties(OutboundEvent outboundEvent)
    {
        ArgumentNullException.ThrowIfNull(outboundEvent);

        return new BasicProperties
        {
            MessageId = outboundEvent.EventId.ToString(),
            Type = outboundEvent.EventType,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(outboundEvent.OccurredAt.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                [SchemaVersionHeader] = outboundEvent.SchemaVersion,
            },
        };
    }

    public async ValueTask DisposeAsync()
    {
        await CloseChannelAsync();

        gate.Dispose();
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (channel is { IsOpen: true })
        {
            return channel;
        }

        await CloseChannelAsync();

        var connection = connectionFactory();

        var newChannel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        try
        {
            await newChannel.ExchangeDeclareAsync(
                options.ExchangeName,
                ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);
        }
        catch
        {
            await newChannel.DisposeAsync();

            throw;
        }

        channel = newChannel;

        return newChannel;
    }

    private async Task CloseChannelAsync()
    {
        if (channel is null)
        {
            return;
        }

        var closing = channel;
        channel = null;

        try
        {
            await closing.DisposeAsync();
        }
        catch (Exception)
        {
            // Disposing a broken channel can throw; there is nothing to recover.
        }
    }
}
