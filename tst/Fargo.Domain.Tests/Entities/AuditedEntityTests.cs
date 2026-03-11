using Fargo.Domain.Entities;

namespace Fargo.Domain.Tests.Entities;

public sealed class AuditedEntityTests
{
    private sealed class TestAuditedEntity : AuditedEntity { }

    [Fact]
    public void CreatedAt_Should_BeInitializedWithUtcNow()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var entity = new TestAuditedEntity();

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(entity.CreatedAt, before, after);
    }

    [Fact]
    public void CreatedByGuid_Should_AssignValue_WhenInitialized()
    {
        // Arrange
        var createdByGuid = Guid.NewGuid();

        // Act
        var entity = new TestAuditedEntity
        {
            CreatedByGuid = createdByGuid
        };

        // Assert
        Assert.Equal(createdByGuid, entity.CreatedByGuid);
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
    public void Should_AssignInitialAuditingProperties_WhenInitialized()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var createdByGuid = Guid.NewGuid();

        // Act
        var entity = new TestAuditedEntity
        {
            CreatedAt = createdAt,
            CreatedByGuid = createdByGuid
        };

        // Assert
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal(createdByGuid, entity.CreatedByGuid);
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
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
}