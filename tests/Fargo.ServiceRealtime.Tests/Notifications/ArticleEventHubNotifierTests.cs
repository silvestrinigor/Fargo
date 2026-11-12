using Fargo.Application.IntegrationEvents;
using Fargo.ServiceRealtime.Connections;
using Fargo.ServiceRealtime.Hubs;
using Fargo.ServiceRealtime.Notifications;
using Fargo.ServiceRealtime.Tests.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.ServiceRealtime.Tests.Notifications;

public class ArticleEventHubNotifierTests
{
    private static readonly Guid PartitionA = Guid.NewGuid();
    private static readonly Guid PartitionB = Guid.NewGuid();

    private readonly RealtimeConnectionRegistry registry = new();
    private readonly IHubContext<FargoHub, IFargoRealtimeClient> hubContext =
        Substitute.For<IHubContext<FargoHub, IFargoRealtimeClient>>();
    private readonly IFargoRealtimeClient targetedClients = Substitute.For<IFargoRealtimeClient>();

    private IReadOnlyList<string>? targetedIds;

    public ArticleEventHubNotifierTests()
    {
        hubContext.Clients
            .Clients(Arg.Do<IReadOnlyList<string>>(ids => targetedIds = ids))
            .Returns(targetedClients);
    }

    private ArticleEventHubNotifier CreateNotifier()
        => new(hubContext, registry, NullLogger<ArticleEventHubNotifier>.Instance);

    private static ArticleEventMessage NewEvent(string eventType, params Guid[] partitions)
        => new(Guid.CreateVersion7(), eventType, DateTimeOffset.UtcNow, Guid.NewGuid(), partitions);

    [Fact]
    public async Task HandleAsync_ShouldNotifyOnlyConnectionsWithAccessToAPartition()
    {
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("a", PartitionA));
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("b", PartitionB));
        var message = NewEvent(ArticleEventTypes.Edited, PartitionA);

        await CreateNotifier().HandleAsync(message);

        Assert.Equal(["a"], targetedIds);
        await targetedClients.Received(1).EntityChanged(Arg.Is<EntityChangedNotification>(n =>
            n.EntityType == EntityChangedNotification.ArticleEntityType &&
            n.Guid == message.ArticleGuid &&
            n.ChangeType == EntityChangedNotification.ChangeTypes.Edited &&
            n.EventId == message.EventId &&
            n.OccurredAt == message.OccurredAt));
    }

    [Fact]
    public async Task HandleAsync_WhenNoConnectionHasAccess_ShouldSendNothing()
    {
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("b", PartitionB));

        await CreateNotifier().HandleAsync(NewEvent(ArticleEventTypes.Created, PartitionA));

        Assert.Null(targetedIds);
        await targetedClients.DidNotReceiveWithAnyArgs().EntityChanged(default!);
    }

    [Fact]
    public async Task HandleAsync_WhenTheEventHasNoPartitions_ShouldSendNothing()
    {
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("a", PartitionA));

        await CreateNotifier().HandleAsync(NewEvent(ArticleEventTypes.Created));

        await targetedClients.DidNotReceiveWithAnyArgs().EntityChanged(default!);
    }

    [Fact]
    public async Task HandleAsync_WhenTheEventTypeIsUnknown_ShouldSendNothing()
    {
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("a", PartitionA));

        await CreateNotifier().HandleAsync(NewEvent("article.archived", PartitionA));

        await targetedClients.DidNotReceiveWithAnyArgs().EntityChanged(default!);
    }

    [Theory]
    [InlineData(ArticleEventTypes.Created, EntityChangedNotification.ChangeTypes.Created)]
    [InlineData(ArticleEventTypes.Edited, EntityChangedNotification.ChangeTypes.Edited)]
    [InlineData(ArticleEventTypes.Deleted, EntityChangedNotification.ChangeTypes.Deleted)]
    public void FromArticleEvent_ShouldMapTheChangeType(string eventType, string expected)
    {
        var notification = EntityChangedNotification.FromArticleEvent(NewEvent(eventType, PartitionA));

        Assert.NotNull(notification);
        Assert.Equal(expected, notification.ChangeType);
    }
}
