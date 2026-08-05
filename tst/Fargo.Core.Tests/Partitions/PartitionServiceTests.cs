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
        partitionRepository.GetDescendantGuidsAsync(partition2.Guid).Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await partitionService.ValidateParentPartitionAssignmentAsync(partition1, partition2);
    }
}
