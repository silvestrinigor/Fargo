using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using Testcontainers.RabbitMq;

namespace Fargo.Infrastructure.Tests.Messaging;

public class RabbitMqArticleEventConsumerParseTests
{
    private static ArticleEventMessage NewMessage()
        => new(Guid.CreateVersion7(), ArticleEventTypes.Created, DateTimeOffset.UtcNow, Guid.NewGuid(), [Guid.NewGuid()]);

    private static BasicProperties PropertiesWithVersion(object? version)
        => new()
        {
            Headers = version is null
                ? null
                : new Dictionary<string, object?> { [RabbitMqEventPublisher.SchemaVersionHeader] = version },
        };

    [Fact]
    public void ParseDelivery_WhenTheMessageIsCurrent_ShouldReturnIt()
    {
        var message = NewMessage();
        var body = Encoding.UTF8.GetBytes(IntegrationEventJson.Serialize(message));

        var parsed = RabbitMqArticleEventConsumer.ParseDelivery(
            body, PropertiesWithVersion(ArticleEventMessage.SchemaVersion), out var dropReason);

        Assert.NotNull(parsed);
        Assert.Null(dropReason);
        Assert.Equal(message.EventId, parsed.EventId);
        Assert.Equal(message.PartitionGuids, parsed.PartitionGuids);
    }

    [Fact]
    public void ParseDelivery_WhenTheVersionHeaderIsMissing_ShouldStillReadTheBody()
    {
        var body = Encoding.UTF8.GetBytes(IntegrationEventJson.Serialize(NewMessage()));

        Assert.NotNull(RabbitMqArticleEventConsumer.ParseDelivery(body, PropertiesWithVersion(null), out _));
    }

    [Fact]
    public void ParseDelivery_WhenTheSchemaVersionDiffers_ShouldDropTheMessage()
    {
        var body = Encoding.UTF8.GetBytes(IntegrationEventJson.Serialize(NewMessage()));

        var parsed = RabbitMqArticleEventConsumer.ParseDelivery(body, PropertiesWithVersion(1), out var dropReason);

        Assert.Null(parsed);
        Assert.Contains("schema version", dropReason);
    }

    [Fact]
    public void ParseDelivery_WhenTheBodyIsInvalid_ShouldDropTheMessage()
    {
        var parsed = RabbitMqArticleEventConsumer.ParseDelivery(
            "nope"u8, PropertiesWithVersion(ArticleEventMessage.SchemaVersion), out var dropReason);

        Assert.Null(parsed);
        Assert.NotNull(dropReason);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(5, 16)]
    [InlineData(6, 30)]
    [InlineData(1000, 30)]
    public void GetRetryDelay_ShouldDoubleUpToTheMaximum(int consecutiveFailures, int expectedSeconds)
    {
        var settings = new RabbitMqConsumerOptions
        {
            RetryDelay = TimeSpan.FromSeconds(1),
            MaxRetryDelay = TimeSpan.FromSeconds(30),
        };

        Assert.Equal(
            TimeSpan.FromSeconds(expectedSeconds),
            RabbitMqArticleEventConsumer.GetRetryDelay(settings, consecutiveFailures));
    }
}

/// <summary>
/// Round trips against a real RabbitMQ broker in a throwaway container (Docker
/// required).
/// </summary>
public sealed class RabbitMqArticleEventConsumerTests : IAsyncLifetime
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

    [Fact]
    public async Task Consumer_ShouldHandEveryPublishedArticleEventToTheHandlerInOrder()
    {
        const string exchange = "fargo.events.consumer";
        var eventOptions = Options.Create(new RabbitMqEventOptions { ExchangeName = exchange });
        var handler = new RecordingHandler();

        using var consumer = new RabbitMqArticleEventConsumer(
            () => connection,
            handler,
            eventOptions,
            Options.Create(new RabbitMqConsumerOptions()),
            TimeProvider.System,
            NullLogger<RabbitMqArticleEventConsumer>.Instance);

        await consumer.StartAsync(CancellationToken.None);

        try
        {
            await using var publisher = new RabbitMqEventPublisher(() => connection, eventOptions);

            // The consumer subscribes in the background; publish until the
            // first event arrives so the test does not race the subscription.
            var first = await PublishUntilReceivedAsync(publisher, handler);

            var second = NewEvent(ArticleEventTypes.Deleted);
            await publisher.PublishAsync(ToOutbound(second));

            Assert.True(await WaitUntilAsync(() => handler.Received.Any(m => m.EventId == second.EventId)));

            var received = handler.Received.ToList();
            Assert.True(received.FindIndex(m => m.EventId == first.EventId) < received.FindIndex(m => m.EventId == second.EventId));
            Assert.Equal(second.PartitionGuids, received.Single(m => m.EventId == second.EventId).PartitionGuids);
        }
        finally
        {
            await consumer.StopAsync(CancellationToken.None);
        }
    }

    private static ArticleEventMessage NewEvent(string eventType)
        => new(Guid.CreateVersion7(), eventType, DateTimeOffset.UtcNow, Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);

    private static OutboundEvent ToOutbound(ArticleEventMessage message)
        => new(message.EventId, message.EventType, ArticleEventMessage.SchemaVersion, message.OccurredAt, IntegrationEventJson.Serialize(message));

    private static async Task<ArticleEventMessage> PublishUntilReceivedAsync(RabbitMqEventPublisher publisher, RecordingHandler handler)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var message = NewEvent(ArticleEventTypes.Created);
            await publisher.PublishAsync(ToOutbound(message));

            if (await WaitUntilAsync(() => handler.Received.Any(m => m.EventId == message.EventId), TimeSpan.FromMilliseconds(200)))
            {
                return message;
            }
        }

        throw new TimeoutException("The consumer never received an event.");
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(50);
        }

        return condition();
    }

    private sealed class RecordingHandler : IArticleEventHandler
    {
        public ConcurrentQueue<ArticleEventMessage> Received { get; } = new();

        public Task HandleAsync(ArticleEventMessage message, CancellationToken cancellationToken = default)
        {
            Received.Enqueue(message);

            return Task.CompletedTask;
        }
    }
}
