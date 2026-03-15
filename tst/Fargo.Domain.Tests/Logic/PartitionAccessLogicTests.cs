using Fargo.Domain.Logics;

namespace Fargo.Domain.Tests.Logics;

public sealed class PartitionAccessLogicTests
{
    [Fact]
    public void HasAccess_ForPartitioned_Should_ReturnTrue_When_EntityHasNoPartitions()
    {
        // Arrange
        var partitioned = new TestPartitioned([]);
        var partitionUser = new TestPartitionUser([]);

        // Act
        var result = partitioned.HasAccess(partitionUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ReturnFalse_When_EntityHasPartitions_And_UserHasNoAccess()
    {
        // Arrange
        var partition = new TestPartition();
        var partitioned = new TestPartitioned([partition]);
        var partitionUser = new TestPartitionUser([]);

        // Act
        var result = partitioned.HasAccess(partitionUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ReturnTrue_When_UserHasAccessTo_EntityPartition()
    {
        // Arrange
        var partition = new TestPartition();
        var partitioned = new TestPartitioned([partition]);
        var partitionUser = new TestPartitionUser([new TestPartitionAccess(partition)]);

        // Act
        var result = partitioned.HasAccess(partitionUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ReturnTrue_When_UserHasAccessTo_AtLeastOnePartition()
    {
        // Arrange
        var partition1 = new TestPartition();
        var partition2 = new TestPartition();
        var partition3 = new TestPartition();

        var partitioned = new TestPartitioned([partition1, partition2]);
        var partitionUser = new TestPartitionUser(
        [
            new TestPartitionAccess(partition3),
            new TestPartitionAccess(partition2)
        ]);

        // Act
        var result = partitioned.HasAccess(partitionUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ReturnFalse_When_UserHasAccessTo_NoneOfThePartitions()
    {
        // Arrange
        var partition1 = new TestPartition();
        var partition2 = new TestPartition();
        var userPartition1 = new TestPartition();
        var userPartition2 = new TestPartition();

        var partitioned = new TestPartitioned([partition1, partition2]);
        var partitionUser = new TestPartitionUser(
        [
            new TestPartitionAccess(userPartition1),
            new TestPartitionAccess(userPartition2)
        ]);

        // Act
        var result = partitioned.HasAccess(partitionUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAccess_ForPartition_Should_ReturnTrue_When_UserHasAccessToPartition()
    {
        // Arrange
        var partition = new TestPartition();
        var partitionUser = new TestPartitionUser([new TestPartitionAccess(partition)]);

        // Act
        var result = partition.HasAccess(partitionUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ForPartition_Should_ReturnFalse_When_UserDoesNotHaveAccessToPartition()
    {
        // Arrange
        var partition = new TestPartition();
        var anotherPartition = new TestPartition();
        var partitionUser = new TestPartitionUser([new TestPartitionAccess(anotherPartition)]);

        // Act
        var result = partition.HasAccess(partitionUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAccess_ForPartition_Should_ReturnFalse_When_UserHasNoPartitions()
    {
        // Arrange
        var partition = new TestPartition();
        var partitionUser = new TestPartitionUser([]);

        // Act
        var result = partition.HasAccess(partitionUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ThrowArgumentNullException_When_PartitionedIsNull()
    {
        // Arrange
        IPartitioned? partitioned = null;
        var partitionUser = new TestPartitionUser([]);

        // Act
        var action = () => partitioned!.HasAccess(partitionUser);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void HasAccess_ForPartitioned_Should_ThrowArgumentNullException_When_PartitionUserIsNull()
    {
        // Arrange
        var partitioned = new TestPartitioned([]);
        IPartitionUser? partitionUser = null;

        // Act
        var action = () => partitioned.HasAccess(partitionUser!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void HasAccess_ForPartition_Should_ThrowArgumentNullException_When_PartitionIsNull()
    {
        // Arrange
        IPartition? partition = null;
        var partitionUser = new TestPartitionUser([]);

        // Act
        var action = () => partition!.HasAccess(partitionUser);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void HasAccess_ForPartition_Should_ThrowArgumentNullException_When_PartitionUserIsNull()
    {
        // Arrange
        var partition = new TestPartition();
        IPartitionUser? partitionUser = null;

        // Act
        var action = () => partition.HasAccess(partitionUser!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    private sealed class TestPartition : IPartition
    {
    }

    private sealed class TestPartitionAccess(IPartition partition) : IPartitionAccess
    {
        public IPartition Partition { get; } = partition;
    }

    private sealed class TestPartitionUser(
        IReadOnlyCollection<IPartitionAccess> partitionAccesses
    ) : IPartitionUser
    {
        public IReadOnlyCollection<IPartitionAccess> PartitionAccesses { get; } = partitionAccesses;
    }

    private sealed class TestPartitioned(
        IReadOnlyCollection<IPartition> partitions
    ) : IPartitioned
    {
        public IReadOnlyCollection<IPartition> Partitions { get; } = partitions;
    }
}