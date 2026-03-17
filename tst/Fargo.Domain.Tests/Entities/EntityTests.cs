using Fargo.Domain.Entities;

namespace Fargo.Domain.Tests.Entities;

public sealed class EntityTests
{
    private sealed class TestEntity : Entity { }

    private sealed class AnotherEntity : Entity { }

    [Fact]
    public void Guid_Should_ThrowArgumentException_When_InitializedWithEmptyGuid()
    {
        // Arrange / Act
        static TestEntity action() => _ = new TestEntity { Guid = Guid.Empty };

        // Assert
        var exception = Assert.Throws<ArgumentException>((Func<TestEntity>)action);
        Assert.Equal("value", exception.ParamName);
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
    public void Equals_Should_ReturnFalse_When_DifferentType()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new AnotherEntity { Guid = entity1.Guid };

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_SameTypeAndGuid()
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
    public void Equals_Should_ReturnFalse_When_DifferentGuid()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_SameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        var entity1 = new TestEntity { Guid = guid };
        var entity2 = new TestEntity { Guid = guid };

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_DifferentGuid()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_DifferentGuid()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_BothNull()
    {
        // Arrange
        Entity? entity1 = null;
        Entity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_OneIsNull()
    {
        // Arrange
        Entity? entity1 = new TestEntity();
        Entity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnHashCodeFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new TestEntity { Guid = guid };

        // Act
        var hash = entity.GetHashCode();

        // Assert
        Assert.Equal(guid.GetHashCode(), hash);
    }
}
