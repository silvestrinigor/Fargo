using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Core.Tests.ServiceAccounts;

public class ServiceAccountTests
{
    private static ServiceAccount NewAccount()
        => ServiceAccount.Create(new Nameid("svc-tester"), new PasswordHash("hash"));

    [Fact]
    public void Create_ShouldBeActiveWithNoPartitionMembership()
    {
        var account = NewAccount();

        Assert.True(account.IsActive);
        // Unlike a user, a service account is NOT implicitly a member of the
        // global partition; memberships are assigned explicitly.
        Assert.Empty(account.Partitions);
        Assert.Empty(account.PartitionGuids);
    }

    [Fact]
    public void AddPartition_ShouldAddMembershipAndSurfaceInPartitionGuids()
    {
        var account = NewAccount();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        account.AddPartition(partition);

        Assert.Contains(partition.Guid, account.PartitionGuids);
        Assert.Contains(account.Partitions, p => p.PartitionGuid == partition.Guid);
    }

    [Fact]
    public void AddPartition_ShouldBeIdempotent()
    {
        var account = NewAccount();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        account.AddPartition(partition);
        account.AddPartition(partition);

        Assert.Single(account.Partitions, p => p.PartitionGuid == partition.Guid);
    }

    [Fact]
    public void AddPartition_WhenNull_ShouldThrow()
    {
        var account = NewAccount();

        Assert.Throws<ArgumentNullException>(() => account.AddPartition(null!));
    }

    [Fact]
    public void RemovePartition_ShouldRemoveMembership()
    {
        var account = NewAccount();
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);
        account.AddPartition(partition);

        account.RemovePartition(partition.Guid);

        Assert.DoesNotContain(partition.Guid, account.PartitionGuids);
        Assert.Empty(account.Partitions);
    }

    [Fact]
    public void RemovePartition_WhenNotAMember_ShouldDoNothing()
    {
        var account = NewAccount();

        // Must not throw when the account is not a member of the partition.
        account.RemovePartition(Guid.NewGuid());

        Assert.Empty(account.Partitions);
    }

    [Fact]
    public void PartitionMembership_ShouldBeIndependentFromPartitionAccess()
    {
        var account = NewAccount();
        var global = Partition.CreateGlobalPartition(default);
        var membership = Partition.CreatePartition(new Name("Member"), global);
        var accessGuid = Guid.NewGuid();

        account.AddPartition(membership);
        account.AddPartitionAccess(accessGuid);

        // Membership drives PartitionGuids; access drives PartitionAccessGuids.
        Assert.Equal([membership.Guid], account.PartitionGuids);
        Assert.Equal([accessGuid], account.PartitionAccessGuids);
    }

    [Fact]
    public void AddPartitionAccess_ShouldBeIdempotent()
    {
        var account = NewAccount();
        var accessGuid = Guid.NewGuid();

        account.AddPartitionAccess(accessGuid);
        account.AddPartitionAccess(accessGuid);

        Assert.Single(account.PartitionAccessGuids, g => g == accessGuid);
    }

    [Fact]
    public void Deactivate_ShouldDeactivateAndBumpAuthVersion()
    {
        var account = NewAccount();
        var before = account.AuthVersion;

        account.Deactivate();

        Assert.False(account.IsActive);
        Assert.NotEqual(before, account.AuthVersion);
    }

    [Fact]
    public void GetEntityType_ShouldBeServiceAccount()
    {
        var account = NewAccount();

        Assert.Equal(Fargo.Core.Entities.EntityType.ServiceAccount, account.GetEntityType());
    }
}
