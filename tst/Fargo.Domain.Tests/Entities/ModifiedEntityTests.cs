using Fargo.Domain.Entities;

namespace Fargo.Domain.Tests.Entities;

public sealed class ModifiedEntityTests
{
    private sealed class TestModifiedEntity : ModifiedEntity
    {
    }

    [Fact]
    public void EditedByGuid_Should_BeNull_ByDefault()
    {
        // Arrange
        var entity = new TestModifiedEntity();

        // Act
        var result = entity.EditedByGuid;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MarkAsEdited_Should_Set_EditedByGuid()
    {
        // Arrange
        var entity = new TestModifiedEntity();
        var userGuid = Guid.NewGuid();

        // Act
        entity.MarkAsEdited(userGuid);

        // Assert
        Assert.Equal(userGuid, entity.EditedByGuid);
    }

    [Fact]
    public void MarkAsEdited_Should_Update_EditedByGuid_When_Called_Multiple_Times()
    {
        // Arrange
        var entity = new TestModifiedEntity();
        var firstUserGuid = Guid.NewGuid();
        var secondUserGuid = Guid.NewGuid();

        // Act
        entity.MarkAsEdited(firstUserGuid);
        entity.MarkAsEdited(secondUserGuid);

        // Assert
        Assert.Equal(secondUserGuid, entity.EditedByGuid);
    }

    [Fact]
    public void MarkAsEdited_Should_Accept_Guid_Empty()
    {
        // Arrange
        var entity = new TestModifiedEntity();

        // Act
        entity.MarkAsEdited(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, entity.EditedByGuid);
    }
}
