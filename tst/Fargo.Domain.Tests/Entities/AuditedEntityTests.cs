using Fargo.Domain.Entities;

namespace Fargo.Domain.Tests.Entities;

public sealed class AuditedEntityTests
{
    private sealed class TestAuditedEntity : AuditedEntity { }

    [Fact]
    public void CreatedAt_Should_BeDefault_ByDefault()
    {
        // Arrange / Act
        var entity = new TestAuditedEntity();

        // Assert
        Assert.Equal(default, entity.CreatedAt);
    }

    [Fact]
    public void CreatedByGuid_Should_BeEmpty_ByDefault()
    {
        // Arrange / Act
        var entity = new TestAuditedEntity();

        // Assert
        Assert.Equal(Guid.Empty, entity.CreatedByGuid);
    }

    [Fact]
    public void EditedAt_Should_BeNull_ByDefault()
    {
        // Arrange / Act
        var entity = new TestAuditedEntity();

        // Assert
        Assert.Null(entity.EditedAt);
    }

    [Fact]
    public void EditedByGuid_Should_BeNull_ByDefault()
    {
        // Arrange / Act
        var entity = new TestAuditedEntity();

        // Assert
        Assert.Null(entity.EditedByGuid);
    }

    [Fact]
    public void MarkAsCreated_Should_SetCreatedAt()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var userGuid = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;

        // Act
        entity.MarkAsCreated(userGuid);

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(entity.CreatedAt, before, after);
    }

    [Fact]
    public void MarkAsCreated_Should_SetCreatedByGuid()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var userGuid = Guid.NewGuid();

        // Act
        entity.MarkAsCreated(userGuid);

        // Assert
        Assert.Equal(userGuid, entity.CreatedByGuid);
    }

    [Fact]
    public void MarkAsCreated_Should_AllowGuidEmpty()
    {
        // Arrange
        var entity = new TestAuditedEntity();

        // Act
        entity.MarkAsCreated(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, entity.CreatedByGuid);
        Assert.NotEqual(default, entity.CreatedAt);
    }

    [Fact]
    public void MarkAsEdited_Should_SetEditedAt()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var before = DateTimeOffset.UtcNow;

        // Act
        entity.MarkAsEdited(Guid.NewGuid());

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.NotNull(entity.EditedAt);
        Assert.InRange(entity.EditedAt!.Value, before, after);
    }

    [Fact]
    public void MarkAsEdited_Should_SetEditedByGuid()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var userGuid = Guid.NewGuid();

        // Act
        entity.MarkAsEdited(userGuid);

        // Assert
        Assert.Equal(userGuid, entity.EditedByGuid);
    }

    [Fact]
    public void MarkAsEdited_Should_SetAllModificationAuditInformation()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var userGuid = Guid.NewGuid();

        // Act
        entity.MarkAsEdited(userGuid);

        // Assert
        Assert.NotNull(entity.EditedAt);
        Assert.Equal(userGuid, entity.EditedByGuid);
    }

    [Fact]
    public void MarkAsEdited_Should_UpdateAuditInformation_WhenCalledMultipleTimes()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var firstUser = Guid.NewGuid();
        var secondUser = Guid.NewGuid();

        entity.MarkAsEdited(firstUser);

        var firstEditedAt = entity.EditedAt;

        // Act
        entity.MarkAsEdited(secondUser);

        // Assert
        Assert.Equal(secondUser, entity.EditedByGuid);
        Assert.NotNull(entity.EditedAt);
        Assert.True(entity.EditedAt >= firstEditedAt);
    }

    [Fact]
    public void MarkAsCreated_Should_NotSetEditedInformation()
    {
        // Arrange
        var entity = new TestAuditedEntity();

        // Act
        entity.MarkAsCreated(Guid.NewGuid());

        // Assert
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
    }
}