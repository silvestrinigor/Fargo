using Fargo.Application.Common;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Repositories;

public class PartitionRepositoryTests
{
    private static FargoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FargoDbContext(options);
    }

    /// <summary>
    /// Regression: the global (root) partition must be returned when the actor
    /// has direct access to it. Previously the repository filter only matched
    /// partitions whose parent was in the access set and excluded roots, so the
    /// global partition was never listed even for an admin that could access it.
    /// </summary>
    [Fact]
    public async Task GetManyInfo_WhenAccessSetContainsGlobalPartition_ReturnsGlobalPartition()
    {
        await using var context = NewContext();

        var global = Partition.CreateGlobalPartition(Name.FromString("Global"));
        var child = Partition.CreatePartition(Name.FromString("Child"), global);

        context.Partitions.AddRange(global, child);
        await context.SaveChangesAsync();

        var repository = new PartitionRepository(context);

        // Actor has direct access to the global partition only.
        var accessGuids = new[] { FargoCoreWellKnowGuids.GlobalPartitionGuid };

        var result = await repository.GetManyInfo(
            new Pagination(new Page(1), new Limit(50)),
            accessGuids);

        var guids = result.Select(p => p.Guid).ToList();

        Assert.Contains(FargoCoreWellKnowGuids.GlobalPartitionGuid, guids);
        // The child of an accessible partition is still returned.
        Assert.Contains(child.Guid, guids);
    }

    /// <summary>
    /// When the actor only has access to a non-root partition, that partition
    /// itself is returned (direct access) and so are its children, but unrelated
    /// partitions are not.
    /// </summary>
    [Fact]
    public async Task GetManyInfo_WhenAccessSetContainsChildPartition_ReturnsThatPartitionAndItsChildren()
    {
        await using var context = NewContext();

        var global = Partition.CreateGlobalPartition(Name.FromString("Global"));
        var branch = Partition.CreatePartition(Name.FromString("Branch"), global);
        var leaf = Partition.CreatePartition(Name.FromString("Leaf"), branch);
        var other = Partition.CreatePartition(Name.FromString("Other"), global);

        context.Partitions.AddRange(global, branch, leaf, other);
        await context.SaveChangesAsync();

        var repository = new PartitionRepository(context);

        var accessGuids = new[] { branch.Guid };

        var result = await repository.GetManyInfo(
            new Pagination(new Page(1), new Limit(50)),
            accessGuids);

        var guids = result.Select(p => p.Guid).ToList();

        Assert.Contains(branch.Guid, guids);   // direct access
        Assert.Contains(leaf.Guid, guids);     // child of accessible
        Assert.DoesNotContain(global.Guid, guids);
        Assert.DoesNotContain(other.Guid, guids);
    }
}
