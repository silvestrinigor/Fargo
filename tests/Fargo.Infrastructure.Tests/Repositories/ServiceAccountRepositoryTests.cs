using Fargo.Application.Common.Paging;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class ServiceAccountRepositoryTests
{
    private static ServiceAccount NewAccount(string clientId)
        => Actor.NewServiceAccount(new Nameid(clientId), new PasswordHash("hash")).ServiceAccount!;

    [Fact]
    public async Task GetByGuid_ShouldReturnAccountWithPartitionMembership()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ServiceAccountRepository(context);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);
        context.Partitions.AddRange(global, partition);

        var account = NewAccount("svc");
        account.Actor.AddPartition(partition);
        repo.Add(account);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(account.ActorGuid);

        Assert.NotNull(found);
        Assert.Contains(partition.Guid, found!.Actor.PartitionGuids);
    }

    [Fact]
    public async Task GetInfoByGuid_WhenActorHasMembershipPartition_ShouldReturnProjection()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ServiceAccountRepository(context);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);
        context.Partitions.AddRange(global, partition);

        var account = NewAccount("svc");
        account.Actor.AddPartition(partition);
        repo.Add(account);
        await context.SaveChangesAsync();

        var dto = await repo.GetInfoByGuidAsync(account.ActorGuid, [partition.Guid]);

        Assert.NotNull(dto);
        Assert.Equal(account.ActorGuid, dto!.Guid);
        Assert.Equal("svc", dto.ClientId);
        Assert.Contains(partition.Guid, dto.PartitionGuids);
    }

    [Fact]
    public async Task GetInfoByGuid_WhenActorLacksMembershipPartition_ShouldReturnNull()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ServiceAccountRepository(context);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);
        context.Partitions.AddRange(global, partition);

        var account = NewAccount("svc");
        account.Actor.AddPartition(partition);
        repo.Add(account);
        await context.SaveChangesAsync();

        // Actor can only reach an unrelated partition -> the account is not visible.
        var dto = await repo.GetInfoByGuidAsync(account.ActorGuid, [Guid.NewGuid()]);

        Assert.Null(dto);
    }

    [Fact]
    public async Task GetInfoByGuid_WhenNoPartitionFilter_ShouldReturnProjection()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ServiceAccountRepository(context);

        var account = NewAccount("svc");
        repo.Add(account);
        await context.SaveChangesAsync();

        // Null filter = unscoped read: returns the account regardless of membership.
        var dto = await repo.GetInfoByGuidAsync(account.ActorGuid, childOfAnyOfThesePartitions: null);

        Assert.NotNull(dto);
        Assert.Equal(account.ActorGuid, dto!.Guid);
    }

    [Fact]
    public async Task GetManyInfo_ShouldReturnOnlyAccountsInActorsPartitions()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ServiceAccountRepository(context);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var a = Partition.CreatePartition(new Name("a"), global);
        var b = Partition.CreatePartition(new Name("b"), global);
        context.Partitions.AddRange(global, a, b);

        var inA = NewAccount("in-a");
        inA.Actor.AddPartition(a);
        var inB = NewAccount("in-b");
        inB.Actor.AddPartition(b);
        repo.Add(inA);
        repo.Add(inB);
        await context.SaveChangesAsync();

        var page = await repo.GetManyInfoAsync(
            new Pagination(new Page(1), new Limit(50)),
            childOfAnyOfThesePartitions: [a.Guid]);

        Assert.Equal(1, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Equal(inA.ActorGuid, page.Items.First().Guid);
    }
}
