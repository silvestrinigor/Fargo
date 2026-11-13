using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using Testcontainers.RabbitMq;

namespace Fargo.Infrastructure.Tests.Messaging;

public class RabbitMqEventPublisherPropertiesTests
{
    [Fact]
    public void CreateProperties_ShouldMapTheEventToPersistentJsonMessage()
    {
        var outboundEvent = new OutboundEvent(
            Guid.Parse("6f1c2b3a-0000-7000-8000-000000000001"),
            ArticleEventTypes.Deleted,
            3,
            new DateTimeOffset(2026, 11, 6, 14, 3, 22, TimeSpan.Zero),
            "{}");

        var properties = RabbitMqEventPublisher.CreateProperties(outboundEvent);

        Assert.Equal("6f1c2b3a-0000-7000-8000-000000000001", properties.MessageId);
        Assert.Equal(ArticleEventTypes.Deleted, properties.Type);
        Assert.Equal("application/json", properties.ContentType);
        Assert.Equal("utf-8", properties.ContentEncoding);
        Assert.Equal(DeliveryModes.Persistent, properties.DeliveryMode);
        Assert.Equal(outboundEvent.OccurredAt.ToUnixTimeSeconds(), properties.Timestamp.UnixTime);
        Assert.Equal(3, properties.Headers![RabbitMqEventPublisher.SchemaVersionHeader]);
    }
}

/// <summary>
/// Round trips against a real RabbitMQ broker in a throwaway container (Docker
/// required).
/// </summary>
public sealed class RabbitMqEventPublisherTests : IAsyncLifetime
{
    private readonly RabbitMqContainer container = new RabbitMqBuilder("rabbitmq:4-management")
        .Build();

    private IConnection connection = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        connection = await new ConnectionFactory { Uri = new Uri(container.GetConnectionString()) }
            .CreateConnectionAsync();
    }

    public async Task DisposeAsync()
    {
        await connection.DisposeAsync();

        await container.DisposeAsync();
    }

    private RabbitMqEventPublisher CreatePublisher(string exchangeName)
        => new(
            () => connection,
            Options.Create(new RabbitMqEventOptions { ExchangeName = exchangeName }));

    private static OutboundEvent NewArticleEvent(string eventType)
    {
        var message = new ArticleEventMessage(
            Guid.CreateVersion7(), eventType, DateTimeOffset.UtcNow, Guid.NewGuid(), [Guid.NewGuid()]);

        return new OutboundEvent(
            message.EventId,
            message.EventType,
            ArticleEventMessage.SchemaVersion,
            message.OccurredAt,
            IntegrationEventJson.Serialize(message));
    }

    private async Task<(IChannel Channel, string Queue)> BindQueueAsync(string exchangeName, string bindingKey)
    {
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

        var queue = await channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true);

        await channel.QueueBindAsync(queue.QueueName, exchangeName, bindingKey);

        return (channel, queue.QueueName);
    }

    private static async Task<BasicGetResult?> GetAsync(IChannel channel, string queue)
    {
        for (var i = 0; i < 50; i++)
        {
            if (await channel.BasicGetAsync(queue, autoAck: true) is { } result)
            {
                return result;
            }

            await Task.Delay(100);
        }

        return null;
    }

    [Fact]
    public async Task PublishAsync_ShouldDeliverTheBodyAndPropertiesToBoundQueues()
    {
        const string exchange = "fargo.events.roundtrip";
        var (channel, queue) = await BindQueueAsync(exchange, "article.#");
        await using var _ = channel;
        await using var publisher = CreatePublisher(exchange);
        var outboundEvent = NewArticleEvent(ArticleEventTypes.Created);

        await publisher.PublishAsync(outboundEvent);

        var received = await GetAsync(channel, queue);
        Assert.NotNull(received);
        Assert.Equal(ArticleEventTypes.Created, received.RoutingKey);
        Assert.Equal(outboundEvent.Body, Encoding.UTF8.GetString(received.Body.Span));
        Assert.Equal(outboundEvent.EventId.ToString(), received.BasicProperties.MessageId);
        Assert.Equal(ArticleEventTypes.Created, received.BasicProperties.Type);
        Assert.Equal(DeliveryModes.Persistent, received.BasicProperties.DeliveryMode);
        Assert.Equal(
            ArticleEventMessage.SchemaVersion,
            received.BasicProperties.Headers![RabbitMqEventPublisher.SchemaVersionHeader]);
    }

    [Fact]
    public async Task PublishAsync_ShouldRouteByEventType()
    {
        const string exchange = "fargo.events.routing";
        var (channel, deletedQueue) = await BindQueueAsync(exchange, ArticleEventTypes.Deleted);
        await using var _ = channel;
        await using var publisher = CreatePublisher(exchange);

        await publisher.PublishAsync(NewArticleEvent(ArticleEventTypes.Edited));
        var deleted = NewArticleEvent(ArticleEventTypes.Deleted);
        await publisher.PublishAsync(deleted);

        var received = await GetAsync(channel, deletedQueue);
        Assert.NotNull(received);
        Assert.Equal(deleted.EventId.ToString(), received.BasicProperties.MessageId);
        Assert.Null(await channel.BasicGetAsync(deletedQueue, autoAck: true));
    }

    [Fact]
    public async Task PublishAsync_WhenTheExchangeCannotBeDeclared_ShouldThrowAndRecoverOnALaterPublish()
    {
        const string exchange = "fargo.events.conflict";

        // An exchange of another type with the same name makes the publisher's
        // declaration fail.
        await using (var setup = await connection.CreateChannelAsync())
        {
            await setup.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true, autoDelete: false);
        }

        await using var publisher = CreatePublisher(exchange);

        await Assert.ThrowsAsync<OperationInterruptedException>(
            () => publisher.PublishAsync(NewArticleEvent(ArticleEventTypes.Created)));

        // Once the conflict is gone, the publisher opens a new channel and works.
        await using (var setup = await connection.CreateChannelAsync())
        {
            await setup.ExchangeDeleteAsync(exchange);
        }

        var (channel, queue) = await BindQueueAsync(exchange, "article.#");
        await using var _ = channel;

        await publisher.PublishAsync(NewArticleEvent(ArticleEventTypes.Created));

        Assert.NotNull(await GetAsync(channel, queue));
    }
}
