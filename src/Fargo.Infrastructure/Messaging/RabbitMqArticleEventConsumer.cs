using Fargo.Application.IntegrationEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// Consumes article events from the RabbitMQ topic exchange and hands each one
/// to the registered <see cref="IArticleEventHandler"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each running instance subscribes through its own exclusive, auto-delete,
/// server-named queue bound to <see cref="BindingKey"/>, so every instance
/// receives every event (fan-out) and the queue disappears with the instance.
/// Events published while an instance is not subscribed are not delivered to
/// it; consumers are expected to re-read state after reconnecting.
/// </para>
/// <para>
/// Messages are acknowledged after the handler runs, one at a time and in
/// order. A message that is not a valid current-version article event is logged
/// and dropped, and a handler failure is logged and the event skipped: neither
/// is requeued, so one bad message can never block the queue.
/// </para>
/// <para>
/// The connection is resolved when subscribing, not at construction, so the
/// host starts while the broker is down. When the broker is unreachable, or the
/// channel closes, the consumer subscribes again after a delay that doubles up
/// to <see cref="RabbitMqConsumerOptions.MaxRetryDelay"/>.
/// </para>
/// </remarks>
public sealed class RabbitMqArticleEventConsumer(
    Func<IConnection> connectionFactory,
    IArticleEventHandler handler,
    IOptions<RabbitMqEventOptions> eventOptions,
    IOptions<RabbitMqConsumerOptions> consumerOptions,
    TimeProvider timeProvider,
    ILogger<RabbitMqArticleEventConsumer> logger
) : BackgroundService
{
    public const string BindingKey = "article.*";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Resolving the connection can block while the client retries; leave
        // the synchronous part of startup first so it never delays the host.
        await Task.Yield();

        var settings = consumerOptions.Value;
        var consecutiveFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;

            try
            {
                var reason = await ConsumeUntilClosedAsync(() => consecutiveFailures = 0, stoppingToken);

                consecutiveFailures++;
                delay = GetRetryDelay(settings, consecutiveFailures);

                logger.ConsumerChannelClosed(reason, delay);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // For example the broker is unreachable.
                consecutiveFailures++;
                delay = GetRetryDelay(settings, consecutiveFailures);

                logger.ConsumerSubscribeFailed(exception, delay);
            }

            try
            {
                await Task.Delay(delay, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<string> ConsumeUntilClosedAsync(Action onSubscribed, CancellationToken stoppingToken)
    {
        var exchange = eventOptions.Value.ExchangeName;

        var connection = connectionFactory();

        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var closed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        channel.ChannelShutdownAsync += (_, args) =>
        {
            closed.TrySetResult(args.ReplyText);

            return Task.CompletedTask;
        };

        // Declared exactly as the publisher declares it, so either side may
        // start first.
        await channel.ExchangeDeclareAsync(
            exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var queue = await channel.QueueDeclareAsync(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(queue.QueueName, exchange, BindingKey, cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, consumerOptions.Value.PrefetchCount, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (_, delivery) => OnReceivedAsync(channel, delivery, stoppingToken);

        await channel.BasicConsumeAsync(queue.QueueName, autoAck: false, consumer, stoppingToken);

        onSubscribed();

        logger.ConsumerSubscribed(exchange, BindingKey, queue.QueueName);

        return await closed.Task.WaitAsync(stoppingToken);
    }

    private async Task OnReceivedAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        var message = ParseDelivery(delivery.Body.Span, delivery.BasicProperties, out var dropReason);

        if (message is null)
        {
            logger.ConsumerMessageDropped(delivery.BasicProperties.MessageId, delivery.RoutingKey, dropReason!);
        }
        else
        {
            try
            {
                await handler.HandleAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutting down: leave the message unacknowledged; the queue is
                // deleted with the connection anyway.
                return;
            }
            catch (Exception exception)
            {
                logger.ConsumerHandlerFailed(exception, message.EventId, message.EventType);
            }
        }

        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
    }

    /// <summary>
    /// Returns <see langword="null"/>, with the reason in <paramref name="dropReason"/>, when the message has
    /// another schema version or is not a complete article event.
    /// </summary>
    public static ArticleEventMessage? ParseDelivery(
        ReadOnlySpan<byte> body,
        IReadOnlyBasicProperties properties,
        out string? dropReason)
    {
        ArgumentNullException.ThrowIfNull(properties);

        if (properties.Headers?.TryGetValue(RabbitMqEventPublisher.SchemaVersionHeader, out var version) == true &&
            !Equals(version, ArticleEventMessage.SchemaVersion))
        {
            dropReason = $"schema version {version} is not {ArticleEventMessage.SchemaVersion}";

            return null;
        }

        var message = IntegrationEventJson.DeserializeArticleEvent(body);

        dropReason = message is null ? "the body is not a valid article event" : null;

        return message;
    }

    internal static TimeSpan GetRetryDelay(RabbitMqConsumerOptions settings, int consecutiveFailures)
    {
        var exponent = Math.Min(Math.Max(consecutiveFailures - 1, 0), 30);
        var ticks = settings.RetryDelay.Ticks * Math.Pow(2, exponent);

        return ticks >= settings.MaxRetryDelay.Ticks
            ? settings.MaxRetryDelay
            : TimeSpan.FromTicks((long)ticks);
    }
}
