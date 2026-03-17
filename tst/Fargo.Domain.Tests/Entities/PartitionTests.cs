using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class PartitionTests
{
    [Fact]
    public void Description_Should_BeEmpty_ByDefault()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        var result = partition.Description;

        // Assert
        Assert.Equal(Description.Empty, result);
    }

    [Fact]
    public void IsActive_Should_BeTrue_ByDefault()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        var result = partition.IsActive;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ParentPartitionGuid_Should_BeNull_ByDefault()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        var result = partition.ParentPartitionGuid;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParentPartition_Should_BeNull_ByDefault()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        var result = partition.ParentPartition;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Activate_Should_SetIsActive_ToTrue()
    {
        // Arrange
        var partition = CreatePartition();
        partition.Deactivate();

        // Act
        partition.Activate();

        // Assert
        Assert.True(partition.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActive_ToFalse()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        partition.Deactivate();

        // Assert
        Assert.False(partition.IsActive);
    }

    [Fact]
    public void SetParentPartition_Should_SetParentPartition()
    {
        // Arrange
        var parentPartition = CreatePartition(name: "Parent");
        var partition = CreatePartition(name: "Child");

        // Act
        partition.SetParentPartition(parentPartition);

        // Assert
        Assert.Same(parentPartition, partition.ParentPartition);
    }

    [Fact]
    public void SetParentPartition_Should_SetParentPartitionGuid()
    {
        // Arrange
        var parentPartition = CreatePartition(name: "Parent");
        var partition = CreatePartition(name: "Child");

        // Act
        partition.SetParentPartition(parentPartition);

        // Assert
        Assert.Equal(parentPartition.Guid, partition.ParentPartitionGuid);
    }

    [Fact]
    public void SetParentPartition_Should_ThrowArgumentNullException_When_ParentPartitionIsNull()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        void act() => partition.SetParentPartition(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void ClearParentPartition_Should_SetParentPartition_ToNull()
    {
        // Arrange
        var parentPartition = CreatePartition(name: "Parent");
        var partition = CreatePartition(name: "Child");
        partition.SetParentPartition(parentPartition);

        // Act
        partition.ClearParentPartition();

        // Assert
        Assert.Null(partition.ParentPartition);
    }

    [Fact]
    public void ClearParentPartition_Should_SetParentPartitionGuid_ToNull()
    {
        // Arrange
        var parentPartition = CreatePartition(name: "Parent");
        var partition = CreatePartition(name: "Child");
        partition.SetParentPartition(parentPartition);

        // Act
        partition.ClearParentPartition();

        // Assert
        Assert.Null(partition.ParentPartitionGuid);
    }

    [Fact]
    public void ClearParentPartition_Should_DoNothing_When_ParentPartitionWasNotSet()
    {
        // Arrange
        var partition = CreatePartition();

        // Act
        var exception = Record.Exception(partition.ClearParentPartition);

        // Assert
        Assert.Null(exception);
        Assert.Null(partition.ParentPartition);
        Assert.Null(partition.ParentPartitionGuid);
    }

    private static Partition CreatePartition(
        Guid? guid = null,
        string name = "Partition A",
        Description? description = null,
        bool isActive = true)
    {
        return new Partition
        {
            Guid = guid ?? Guid.NewGuid(),
            Name = new Name(name),
            Description = description ?? Description.Empty,
            IsActive = isActive
        };
    }
}
