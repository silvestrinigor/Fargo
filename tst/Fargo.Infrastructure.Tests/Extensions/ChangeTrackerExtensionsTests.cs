using Fargo.Domain.Entities;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Extensions;

public sealed class ChangeTrackerExtensionsTests
{
    private sealed class TestAuditedEntity : AuditedEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestNonAuditedEntity : Entity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TestAuditedEntity> AuditedEntities => Set<TestAuditedEntity>();

        public DbSet<TestNonAuditedEntity> NonAuditedEntities => Set<TestNonAuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAuditedEntity>().HasKey(x => x.Guid);
            modelBuilder.Entity<TestNonAuditedEntity>().HasKey(x => x.Guid);

            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public async Task ApplyAuditing_Should_SetEditedAtAndEditedByGuid_When_AuditedEntityIsModified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Before"
        };

        context.AuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "After";

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotNull(entity.EditedAt);
        Assert.Equal(currentUserGuid, entity.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_NotSetEditedAtOrEditedByGuid_When_AuditedEntityIsUnchanged()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Value"
        };

        context.AuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_NotThrow_When_TrackedEntityIsNotAudited()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var entity = new TestNonAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Before"
        };

        context.NonAuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "After";

        // Act
        var exception = Record.Exception(() =>
            context.ChangeTracker.ApplyAuditing(currentUserGuid));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateOnlyModifiedAuditedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var modifiedEntity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Modified"
        };

        var unchangedEntity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Unchanged"
        };

        context.AuditedEntities.AddRange(modifiedEntity, unchangedEntity);
        await context.SaveChangesAsync();

        modifiedEntity.Name = "Modified After";

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotNull(modifiedEntity.EditedAt);
        Assert.Equal(currentUserGuid, modifiedEntity.EditedByGuid);

        Assert.Null(unchangedEntity.EditedAt);
        Assert.Null(unchangedEntity.EditedByGuid);
    }
}