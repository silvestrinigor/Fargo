using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Security;

namespace Fargo.Core.Tests.ServiceAccounts;

public class ServiceAccountTests
{
    private static Actor NewAccountActor()
        => Actor.NewServiceAccount(new Nameid("svc-tester"), new PasswordHash("hash"));

    [Fact]
    public void Create_ShouldBeActiveWithNoPartitionMembership()
    {
        var actor = NewAccountActor();

        Assert.True(actor.IsActive);
        // Unlike a user, a service account is NOT implicitly a member of the
        // global partition; memberships are assigned explicitly.
        Assert.Empty(actor.Partitions);
        Assert.Empty(actor.PartitionGuids);
    }

    [Fact]
    public void AddPartition_ShouldAddMembershipAndSurfaceInPartitionGuids()
    {
        var actor = NewAccountActor();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        actor.AddPartition(partition);

        Assert.Contains(partition.Guid, actor.PartitionGuids);
        Assert.Contains(actor.Partitions, p => p.PartitionGuid == partition.Guid);
    }

    [Fact]
    public void AddPartition_ShouldBeIdempotent()
    {
        var actor = NewAccountActor();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        actor.AddPartition(partition);
        actor.AddPartition(partition);

        Assert.Single(actor.Partitions, p => p.PartitionGuid == partition.Guid);
    }

    [Fact]
    public void AddPartition_WhenNull_ShouldThrow()
    {
        var actor = NewAccountActor();

        Assert.Throws<ArgumentNullException>(() => actor.AddPartition(null!));
    }

    [Fact]
    public void RemovePartition_ShouldRemoveMembership()
    {
        var actor = NewAccountActor();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);
        actor.AddPartition(partition);

        actor.RemovePartition(partition.Guid);

        Assert.DoesNotContain(partition.Guid, actor.PartitionGuids);
        Assert.Empty(actor.Partitions);
    }

    [Fact]
    public void RemovePartition_WhenNotAMember_ShouldDoNothing()
    {
        var actor = NewAccountActor();

        // Must not throw when the account is not a member of the partition.
        actor.RemovePartition(Guid.NewGuid());

        Assert.Empty(actor.Partitions);
    }

    [Fact]
    public void PartitionMembership_ShouldBeIndependentFromPartitionAccess()
    {
        var actor = NewAccountActor();
        var global = Partition.CreateGlobalPartition(default);
        var membership = Partition.CreatePartition(new Name("Member"), global);
        var access = Partition.CreatePartition(new Name("Access"), global);

        actor.AddPartition(membership);
        actor.AddPartitionAccess(access);

        // Membership drives PartitionGuids; access drives PartitionAccesses.
        Assert.Equal([membership.Guid], actor.PartitionGuids);
        Assert.Equal([access.Guid], actor.PartitionAccesses.Select(a => a.PartitionGuid));
    }

    [Fact]
    public void AddPartitionAccess_ShouldBeIdempotent()
    {
        var actor = NewAccountActor();
        var global = Partition.CreateGlobalPartition(default);
        var access = Partition.CreatePartition(new Name("Access"), global);

        actor.AddPartitionAccess(access);
        actor.AddPartitionAccess(access);

        Assert.Single(actor.PartitionAccesses, a => a.PartitionGuid == access.Guid);
    }

    [Fact]
    public void Deactivate_ShouldDeactivateActorAndRotateAuthVersion()
    {
        var actor = NewAccountActor();
        var account = actor.ServiceAccount!;
        var before = account.AuthVersion;

        // Deactivation of a service account is a two-part operation: the actor is
        // deactivated and the account's auth version is rotated so previously
        // issued tokens stop validating.
        actor.Deactivate();
        account.RotateAuthVersion();

        Assert.False(actor.IsActive);
        Assert.NotEqual(before, account.AuthVersion);
    }

    [Fact]
    public void GetEntityType_ShouldBeActor()
    {
        var actor = NewAccountActor();

        Assert.Equal(Fargo.Core.Entities.EntityType.Actor, actor.GetEntityType());
    }
}
