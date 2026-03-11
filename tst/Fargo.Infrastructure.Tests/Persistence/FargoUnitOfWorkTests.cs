using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Fargo.Infrastructure.Tests.Persistence;

public sealed class FargoUnitOfWorkTests
{
    private sealed class TestAuditedEntity : AuditedEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestFargoWriteDbContext(
        DbContextOptions<FargoWriteDbContext> options)
        : FargoWriteDbContext(options)
    {
        public DbSet<TestAuditedEntity> AuditedEntities => Set<TestAuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAuditedEntity>().HasKey(x => x.Guid);

            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public async Task SaveChanges_Should_ApplyAuditing_When_AuditedEntityIsModified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FargoWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUser = Substitute.For<ICurrentUser>();
        var currentUserGuid = Guid.NewGuid();

        currentUser.UserGuid.Returns(currentUserGuid);

        await using var context = new TestFargoWriteDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Before"
        };

        context.AuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "After";

        var unitOfWork = new FargoUnitOfWork(context, currentUser);

        // Act
        await unitOfWork.SaveChanges();

        // Assert
        Assert.NotNull(entity.EditedAt);
        Assert.Equal(currentUserGuid, entity.EditedByGuid);
    }

    [Fact]
    public async Task SaveChanges_Should_ReturnAffectedEntriesCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FargoWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserGuid.Returns(Guid.NewGuid());

        await using var context = new TestFargoWriteDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Value"
        };

        context.AuditedEntities.Add(entity);

        var unitOfWork = new FargoUnitOfWork(context, currentUser);

        // Act
        var result = await unitOfWork.SaveChanges();

        // Assert
        Assert.True(result > 0);
    }
}