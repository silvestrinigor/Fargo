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
}