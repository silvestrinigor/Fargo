using Fargo.Core.Partitions;
using Fargo.Core.Shared.Informations;

namespace Fargo.Core.Tests.Partitions;

public class PartitionTests
{
    [Fact]
    public void CreateGlobalPartition_WhenValidParams_ShouldBeGlobalPartition()
    {
        var partition = CreateGlobalPartition();

        Assert.Equal(FargoCoreGuids.GlobalPartitionGuid, partition.Guid);
        Assert.True(partition.IsGlobalPartition);
    }

    [Fact]
    public void CreateGlobalPartition_WhenValidParams_ShouldSetName()
    {
        var name = new Name("New test partition");

        var partition = Partition.CreateGlobalPartition(name);

        Assert.Equal(name, partition.Name);
    }

    [Fact]
    public void CreateNormalPartition_WhenValidParams_ShouldNotBeGlobalPartition()
    {
        var globalPartition = CreateGlobalPartition();

        var partition = CreatePartition(globalPartition);

        Assert.NotEqual(FargoCoreGuids.GlobalPartitionGuid, partition.Guid);
        Assert.False(partition.IsGlobalPartition);
    }

    [Fact]
    public void CreateNormalPartition_WhenValidParams_ShouldSetParentPartition()
    {
        var globalPartition = CreateGlobalPartition();

        var partition = CreatePartition(globalPartition);

        Assert.Equal(globalPartition, partition.ParentPartition);
        Assert.Equal(globalPartition.Guid, partition.ParentPartitionGuid);
    }

    [Fact]
    public void CreateNormalPartition_WhenValidParams_ShouldSetName()
    {
        var globalPartition = CreateGlobalPartition();
        var name = new Name("New test partition");

        var partition = Partition.CreatePartition(name, globalPartition);

        Assert.Equal(name, partition.Name);
    }

    [Fact]
    public void SetParentPartition_WhenNewParentPartitionIsValid_ShouldSetParentPartition()
    {
        var globalPartition = CreateGlobalPartition();
        var partition = CreatePartition(globalPartition);
        var partition2 = CreatePartition(globalPartition);

        partition.SetParentPartition(partition2);

        Assert.Equal(partition2, partition.ParentPartition);
        Assert.Equal(partition2.Guid, partition.ParentPartitionGuid);
    }

    [Fact]
    public void SetParentPartition_WhenMemberPartitionIsGlobalPartition_ShouldThrowException()
    {
        var globalPartition = CreateGlobalPartition();
        var partition = CreatePartition(globalPartition);

        void function() => globalPartition.SetParentPartition(partition);

        var ex = Assert.Throws<FargoCoreException>(() => function());
        Assert.Equal(FargoCoreErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void SetParentPartition_WhenPartitionMemberIsEqualParentPartition_ShouldThrowException()
    {
        var globalPartition = CreateGlobalPartition();
        var partition = CreatePartition(globalPartition);

        void function() => partition.SetParentPartition(partition);

        var ex = Assert.Throws<FargoCoreException>(() => function());
        Assert.Equal(FargoCoreErrorType.InvalidArgument, ex.ErrorType);
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
