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
        var globalPartition = CreateGlobalPartition();
        var partition1 = CreatePartition(globalPartition);
        var partition2 = CreatePartition(globalPartition);
        partitionRepository.GetDescendantGuidsAsync(partition2.Guid).Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await partitionService.InsertIntoPartitionAsync(partition1, partition2);

        Assert.Equal(partition1, partition2.ParentPartition);
        Assert.Equal(partition1.Guid, partition2.ParentPartitionGuid);
        Assert.Equal(globalPartition, partition1.ParentPartition);
        Assert.Equal(globalPartition.Guid, partition1.ParentPartitionGuid);
    }

    private static Partition CreateGlobalPartition()
    {
        return Partition.CreateGlobalPartition(new("Global test partition"));
    }

    private static Partition CreatePartition(Partition parentPartition)
    {
        return Partition.CreatePartition(new("New test partition"), parentPartition);
    }

}