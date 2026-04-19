namespace Fargo.Domain.Tests.Entities;

public sealed class EntityTests
{
    private sealed class TestEntity : Entity
    {
    }

    private sealed class OtherTestEntity : Entity
    {
    }

    [Fact]
    public void Guid_Should_BeGenerated_When_NotProvided()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.Guid;

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void Guid_Should_UseProvidedValue_When_NotEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var entity = new TestEntity
        {
            Guid = guid
        };

        // Assert
        Assert.Equal(guid, entity.Guid);
    }

    [Fact]
    public void Guid_Should_ThrowArgumentException_When_Empty()
    {
        // Arrange / Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = new TestEntity
            {
                Guid = Guid.Empty
            };
        });

        // Assert
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Entity Guid cannot be empty.", exception.Message);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_SameReference()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.Equals(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_TypesAreDifferent()
    {
        // Arrange
        var guid = Guid.NewGuid();

        Entity entity1 = new TestEntity { Guid = guid };
        Entity entity2 = new OtherTestEntity { Guid = guid };

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_SameType_And_SameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        var entity1 = new TestEntity { Guid = guid };
        var entity2 = new TestEntity { Guid = guid };

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_SameType_And_DifferentGuid()
    {
        // Arrange
        var entity1 = new TestEntity { Guid = Guid.NewGuid() };
        var entity2 = new TestEntity { Guid = Guid.NewGuid() };

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHashCode_ForSameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        var entity1 = new TestEntity { Guid = guid };
        var entity2 = new TestEntity { Guid = guid };

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_BothAreNull()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_OnlyOneIsNull()
    {
        // Arrange
        TestEntity? entity1 = new();
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_SameType_And_SameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        TestEntity entity1 = new() { Guid = guid };
        TestEntity entity2 = new() { Guid = guid };

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_SameType_And_DifferentGuid()
    {
        // Arrange
        TestEntity entity1 = new() { Guid = Guid.NewGuid() };
        TestEntity entity2 = new() { Guid = Guid.NewGuid() };

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_SameType_And_SameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        TestEntity entity1 = new() { Guid = guid };
        TestEntity entity2 = new() { Guid = guid };

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_SameType_And_DifferentGuid()
    {
        // Arrange
        TestEntity entity1 = new() { Guid = Guid.NewGuid() };
        TestEntity entity2 = new() { Guid = Guid.NewGuid() };

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.True(result);
    }
}
