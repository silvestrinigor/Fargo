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

    private sealed class TestAuditedAggregateMember : Entity, IAuditedAggregateMember
    {
        public string Name { get; set; } = string.Empty;

        public required TestAuditedEntity Parent { get; init; }

        public IAuditedEntity ParentAuditedEntity => Parent;
    }

    private sealed class TestNonAuditedEntity : Entity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TestAuditedEntity> AuditedEntities => Set<TestAuditedEntity>();

        public DbSet<TestAuditedAggregateMember> AuditedAggregateMembers => Set<TestAuditedAggregateMember>();

        public DbSet<TestNonAuditedEntity> NonAuditedEntities => Set<TestNonAuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAuditedEntity>().HasKey(x => x.Guid);
            modelBuilder.Entity<TestAuditedAggregateMember>().HasKey(x => x.Guid);
            modelBuilder.Entity<TestNonAuditedEntity>().HasKey(x => x.Guid);

            modelBuilder
                .Entity<TestAuditedAggregateMember>()
                .HasOne(x => x.Parent);

            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public void ApplyAuditing_Should_SetCreatedAtAndCreatedByGuid_When_AuditedEntityIsAdded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        using var context = new TestDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Created"
        };

        context.Add(entity);

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(currentUserGuid, entity.CreatedByGuid);
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_SetEditedAtAndEditedByGuid_When_AuditedEntityIsModified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var creationUserGuid = Guid.NewGuid();
        var editionUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Before"
        };

        context.Add(entity);
        context.ChangeTracker.ApplyAuditing(creationUserGuid);
        await context.SaveChangesAsync();

        entity.Name = "After";

        // Act
        context.ChangeTracker.ApplyAuditing(editionUserGuid);

        // Assert
        Assert.NotNull(entity.EditedAt);
        Assert.Equal(editionUserGuid, entity.EditedByGuid);
        Assert.Equal(creationUserGuid, entity.CreatedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_NotSetEditedFields_When_AuditedEntityIsUnchanged()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var creationUserGuid = Guid.NewGuid();
        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Value"
        };

        context.Add(entity);
        context.ChangeTracker.ApplyAuditing(creationUserGuid);
        await context.SaveChangesAsync();

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
        Assert.Equal(creationUserGuid, entity.CreatedByGuid);
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

        context.Add(entity);
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

        var creationUserGuid = Guid.NewGuid();
        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var modified = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Modified"
        };

        var unchanged = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Unchanged"
        };

        context.AddRange(modified, unchanged);
        context.ChangeTracker.ApplyAuditing(creationUserGuid);
        await context.SaveChangesAsync();

        modified.Name = "Modified After";

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotNull(modified.EditedAt);
        Assert.Equal(currentUserGuid, modified.EditedByGuid);

        Assert.Null(unchanged.EditedAt);
        Assert.Null(unchanged.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateParentAudit_When_AuditedAggregateMemberIsModified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var creationUserGuid = Guid.NewGuid();
        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var parent = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Parent"
        };

        var child = new TestAuditedAggregateMember
        {
            Guid = Guid.NewGuid(),
            Name = "Child",
            Parent = parent
        };

        context.AddRange(parent, child);
        context.ChangeTracker.ApplyAuditing(creationUserGuid);
        await context.SaveChangesAsync();

        child.Name = "Changed";

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotNull(parent.EditedAt);
        Assert.Equal(currentUserGuid, parent.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateParentAudit_When_AuditedAggregateMemberIsAdded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var creationUserGuid = Guid.NewGuid();
        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var parent = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Parent"
        };

        context.Add(parent);
        context.ChangeTracker.ApplyAuditing(creationUserGuid);
        await context.SaveChangesAsync();

        var child = new TestAuditedAggregateMember
        {
            Guid = Guid.NewGuid(),
            Name = "Child",
            Parent = parent
        };

        context.Add(child);

        // Act
        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        // Assert
        Assert.NotNull(parent.EditedAt);
        Assert.Equal(currentUserGuid, parent.EditedByGuid);
    }

    [Fact]
    public void ApplyAuditing_Should_KeepCreatedByGuidAsGuidEmpty_ByDefault_When_NotApplied()
    {
        // Arrange
        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Entity"
        };

        // Assert
        Assert.Equal(Guid.Empty, entity.CreatedByGuid);
    }
}