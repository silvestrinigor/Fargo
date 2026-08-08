using Fargo.Core.Common;
using Fargo.Core.Partitions;
using NSubstitute;

namespace Fargo.Core.Tests.Partitions;

public class PartitionServiceTests
{
    private readonly IPartitionRepository partitionRepository;
    private readonly PartitionService partitionService;

    public PartitionServiceTests()
    {
        partitionRepository = Substitute.For<IPartitionRepository>();
        partitionService = new PartitionService(partitionRepository);
    }

    [Fact]
    public async Task InsertIntoPartitionAsync_WhenPartitionIsValid_ShouldSetParentPartition()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var partition1 = Partition.CreatePartition(default, globalPartition);
        var partition2 = Partition.CreatePartition(default, globalPartition);
        partitionRepository
        .GetDescendantGuidsAsync(partition2.Guid, includeRoot: false, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(partition1, partition2);
    }

    [Fact]
    public async Task InsertIntoPartitionAsync_WhenCreatesCircularHierarchy_ShouldThrowException()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var partition1 = Partition.CreatePartition(default, globalPartition);
        var partition2 = Partition.CreatePartition(default, partition1);
        var partition3 = Partition.CreatePartition(default, partition2);
        partitionRepository
        .GetDescendantGuidsAsync(partition1.Guid, includeRoot: false, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([partition2.Guid, partition3.Guid]));

        async Task function() => await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(partition3, partition1);

        var ex = await Assert.ThrowsAsync<FargoCoreException>(function);
        Assert.Equal(FargoCoreErrorType.InvalidOperation, ex.ErrorType);
    }
}
