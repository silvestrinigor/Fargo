using Fargo.Core.Common;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Informations;

namespace Fargo.Core.Tests.Partitions;

public class PartitionTests
{
    [Fact]
    public void CreateGlobalPartition_WhenValid_ShouldBeGlobalPartition()
    {
        var partition = Partition.CreateGlobalPartition(default);

        Assert.Equal(FargoCoreWellKnowGuids.GlobalPartitionGuid, partition.Guid);
        Assert.True(partition.IsGlobalPartition);
    }

    [Fact]
    public void CreateGlobalPartition_WhenValid_ShouldSetName()
    {
        var name = new Name("New test partition");

        var partition = Partition.CreateGlobalPartition(name);

        Assert.Equal(name, partition.Name);
    }

    [Fact]
    public void CreateNormalPartition_WhenValid_ShouldNotBeGlobalPartition()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);

        var partition = Partition.CreatePartition(default, globalPartition);

        Assert.NotEqual(FargoCoreWellKnowGuids.GlobalPartitionGuid, partition.Guid);
        Assert.False(partition.IsGlobalPartition);
    }

    [Fact]
    public void CreateNormalPartition_WhenValid_ShouldSetParentPartition()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);

        var partition = Partition.CreatePartition(default, globalPartition);

        Assert.Equal(globalPartition, partition.ParentPartition);
        Assert.Equal(globalPartition.Guid, partition.ParentPartitionGuid);
    }

    [Fact]
    public void CreateNormalPartition_WhenValid_ShouldSetName()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var name = new Name("New test partition");

        var partition = Partition.CreatePartition(name, globalPartition);

        Assert.Equal(name, partition.Name);
    }

    [Fact]
    public void SetParentPartition_WhenValid_ShouldSetParentPartition()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(default, globalPartition);
        var partition2 = Partition.CreatePartition(default, globalPartition);

        partition.SetParentPartition(partition2);

        Assert.Equal(partition2, partition.ParentPartition);
        Assert.Equal(partition2.Guid, partition.ParentPartitionGuid);
    }

    [Fact]
    public void SetParentPartition_WhenMemberPartitionIsGlobalPartition_ShouldThrowException()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(default, globalPartition);

        void function() => globalPartition.SetParentPartition(partition);

        var ex = Assert.Throws<FargoCoreException>(function);
        Assert.Equal(FargoCoreErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void SetParentPartition_WhenPartitionMemberIsEqualParentPartition_ShouldThrowException()
    {
        var globalPartition = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(default, globalPartition);

        void function() => partition.SetParentPartition(partition);

        var ex = Assert.Throws<FargoCoreException>(function);
        Assert.Equal(FargoCoreErrorType.InvalidOperation, ex.ErrorType);
    }
}
