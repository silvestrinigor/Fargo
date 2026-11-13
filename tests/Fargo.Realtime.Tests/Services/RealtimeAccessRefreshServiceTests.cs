using Fargo.Core.Entities.Actors;
using Fargo.Realtime.Connections;
using Fargo.Realtime.Services;
using Fargo.Realtime.Tests.Connections;

namespace Fargo.Realtime.Tests.Services;

public class RealtimeAccessRefreshServiceTests
{
    private static readonly Guid PartitionA = Guid.NewGuid();
    private static readonly Guid PartitionB = Guid.NewGuid();

    [Fact]
    public async Task RefreshAsync_ShouldReplaceTheAccessOfEveryConnectionOfTheActor()
    {
        var actor = Guid.NewGuid();
        var registry = new RealtimeConnectionRegistry();
        var first = RealtimeConnectionRegistryTests.NewConnection("c1", actor, [PartitionA]);
        var second = RealtimeConnectionRegistryTests.NewConnection("c2", actor, [PartitionA]);
        registry.Add(first);
        registry.Add(second);

        await RealtimeAccessRefreshService.RefreshAsync(
            registry,
            (_, _, _) => Task.FromResult<IReadOnlySet<Guid>?>(new HashSet<Guid> { PartitionB }),
            CancellationToken.None);

        Assert.All([first, second], c =>
        {
            Assert.False(c.CanSee([PartitionA]));
            Assert.True(c.CanSee([PartitionB]));
        });
    }

    [Fact]
    public async Task RefreshAsync_WhenTheActorIsGoneOrInactive_ShouldAbortItsConnections()
    {
        var aborted = new List<string>();
        var registry = new RealtimeConnectionRegistry();
        var gone = Guid.NewGuid();
        var active = Guid.NewGuid();
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("gone", gone, [PartitionA], () => aborted.Add("gone")));
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("active", active, [PartitionA], () => aborted.Add("active")));

        await RealtimeAccessRefreshService.RefreshAsync(
            registry,
            (actorGuid, _, _) => Task.FromResult<IReadOnlySet<Guid>?>(
                actorGuid == gone ? null : new HashSet<Guid> { PartitionA }),
            CancellationToken.None);

        Assert.Equal(["gone"], aborted);
    }

    [Fact]
    public async Task RefreshAsync_ShouldResolveEachActorOncePerPass()
    {
        var actor = Guid.NewGuid();
        var registry = new RealtimeConnectionRegistry();
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("c1", actor, [PartitionA]));
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("c2", actor, [PartitionA]));
        registry.Add(RealtimeConnectionRegistryTests.NewConnection("c3", Guid.NewGuid(), [PartitionA]));
        var calls = new List<(Guid, ActorType)>();

        await RealtimeAccessRefreshService.RefreshAsync(
            registry,
            (actorGuid, actorType, _) =>
            {
                calls.Add((actorGuid, actorType));

                return Task.FromResult<IReadOnlySet<Guid>?>(new HashSet<Guid> { PartitionA });
            },
            CancellationToken.None);

        Assert.Equal(2, calls.Count);
        Assert.Single(calls, c => c.Item1 == actor);
    }
}
