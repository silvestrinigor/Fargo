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

    private sealed class TestAuditPropagatorEntity : Entity, IAuditedAggregateMember
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

        public DbSet<TestAuditPropagatorEntity> AuditPropagatorEntities => Set<TestAuditPropagatorEntity>();

        public DbSet<TestNonAuditedEntity> NonAuditedEntities => Set<TestNonAuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAuditedEntity>().HasKey(x => x.Guid);
            modelBuilder.Entity<TestAuditPropagatorEntity>().HasKey(x => x.Guid);
            modelBuilder.Entity<TestNonAuditedEntity>().HasKey(x => x.Guid);

            modelBuilder
                .Entity<TestAuditPropagatorEntity>()
                .HasOne(x => x.Parent);

            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public async Task ApplyAuditing_Should_SetEditedAtAndEditedByGuid_When_AuditedEntityIsModified()
    {
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

        context.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "After";

        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        Assert.NotNull(entity.EditedAt);
        Assert.Equal(currentUserGuid, entity.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_NotSetEditedFields_When_AuditedEntityIsUnchanged()
    {
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

        context.Add(entity);
        await context.SaveChangesAsync();

        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_NotThrow_When_TrackedEntityIsNotAudited()
    {
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

        var exception = Record.Exception(() =>
            context.ChangeTracker.ApplyAuditing(currentUserGuid));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateOnlyModifiedAuditedEntities()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

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
        await context.SaveChangesAsync();

        modified.Name = "Modified After";

        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        Assert.NotNull(modified.EditedAt);
        Assert.Equal(currentUserGuid, modified.EditedByGuid);

        Assert.Null(unchanged.EditedAt);
        Assert.Null(unchanged.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateParentAudit_When_AuditPropagatorIsModified()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var parent = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Parent"
        };

        var child = new TestAuditPropagatorEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Child",
            Parent = parent
        };

        context.AddRange(parent, child);
        await context.SaveChangesAsync();

        child.Name = "Changed";

        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        Assert.NotNull(parent.EditedAt);
        Assert.Equal(currentUserGuid, parent.EditedByGuid);
    }

    [Fact]
    public async Task ApplyAuditing_Should_UpdateParentAudit_When_AuditPropagatorIsAdded()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUserGuid = Guid.NewGuid();

        await using var context = new TestDbContext(options);

        var parent = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Parent"
        };

        context.Add(parent);
        await context.SaveChangesAsync();

        var child = new TestAuditPropagatorEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Child",
            Parent = parent
        };

        context.Add(child);

        context.ChangeTracker.ApplyAuditing(currentUserGuid);

        Assert.NotNull(parent.EditedAt);
        Assert.Equal(currentUserGuid, parent.EditedByGuid);
    }
}