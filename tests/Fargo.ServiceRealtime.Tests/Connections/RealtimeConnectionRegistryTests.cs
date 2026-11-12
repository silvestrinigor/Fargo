using Fargo.Core.Entities.Actors;
using Fargo.ServiceRealtime.Connections;

namespace Fargo.ServiceRealtime.Tests.Connections;

public class RealtimeConnectionRegistryTests
{
    private static readonly Guid PartitionA = Guid.NewGuid();
    private static readonly Guid PartitionB = Guid.NewGuid();
    private static readonly Guid PartitionC = Guid.NewGuid();

    internal static RealtimeConnection NewConnection(
        string id,
        params Guid[] access)
        => NewConnection(id, Guid.NewGuid(), access);

    internal static RealtimeConnection NewConnection(
        string id,
        Guid actorGuid,
        Guid[] access,
        Action? abort = null)
        => new(id, actorGuid, ActorType.User, access.ToHashSet(), abort ?? (() => { }));

    [Fact]
    public void CanSee_WhenAnyPartitionIsAccessible_ShouldBeTrue()
    {
        var connection = NewConnection("c1", PartitionA);

        Assert.True(connection.CanSee([PartitionB, PartitionA]));
    }

    [Fact]
    public void CanSee_WhenNoPartitionIsAccessible_ShouldBeFalse()
    {
        var connection = NewConnection("c1", PartitionA);

        Assert.False(connection.CanSee([PartitionB, PartitionC]));
    }

    [Fact]
    public void CanSee_WhenTheEntityHasNoPartitions_ShouldBeFalse()
    {
        // Matches the API read filter, which never returns an entity without
        // partitions; SecurityPrincipal.HasAccess would allow it.
        var connection = NewConnection("c1", PartitionA);

        Assert.False(connection.CanSee([]));
    }

    [Fact]
    public void UpdateAccess_ShouldReplaceTheAccessUsedByCanSee()
    {
        var connection = NewConnection("c1", PartitionA);

        connection.UpdateAccess(new HashSet<Guid> { PartitionB });

        Assert.False(connection.CanSee([PartitionA]));
        Assert.True(connection.CanSee([PartitionB]));
    }

    [Fact]
    public void GetConnectionIdsThatCanSee_ShouldReturnOnlyConnectionsWithAccess()
    {
        var registry = new RealtimeConnectionRegistry();
        registry.Add(NewConnection("a", PartitionA));
        registry.Add(NewConnection("b", PartitionB));
        registry.Add(NewConnection("ab", PartitionA, PartitionB));
        registry.Add(NewConnection("none"));

        var ids = registry.GetConnectionIdsThatCanSee([PartitionA]);

        Assert.Equal(["a", "ab"], ids.Order());
    }

    [Fact]
    public void GetConnectionIdsThatCanSee_WhenThereAreNoPartitions_ShouldReturnNothing()
    {
        var registry = new RealtimeConnectionRegistry();
        registry.Add(NewConnection("a", PartitionA));

        Assert.Empty(registry.GetConnectionIdsThatCanSee([]));
    }

    [Fact]
    public void Remove_ShouldForgetTheConnection()
    {
        var registry = new RealtimeConnectionRegistry();
        registry.Add(NewConnection("a", PartitionA));

        registry.Remove("a");

        Assert.Equal(0, registry.Count);
        Assert.Empty(registry.GetConnectionIdsThatCanSee([PartitionA]));
    }
}
