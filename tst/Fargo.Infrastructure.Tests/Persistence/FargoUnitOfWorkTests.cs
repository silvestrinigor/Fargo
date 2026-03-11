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

        var creationUser = Substitute.For<ICurrentUser>();
        var creationUserGuid = Guid.NewGuid();
        creationUser.UserGuid.Returns(creationUserGuid);

        await using var context = new TestFargoWriteDbContext(options);

        var entity = new TestAuditedEntity
        {
            Guid = Guid.NewGuid(),
            Name = "Before"
        };

        context.AuditedEntities.Add(entity);

        var creationUnitOfWork = new FargoUnitOfWork(context, creationUser);
        await creationUnitOfWork.SaveChanges();

        var editUser = Substitute.For<ICurrentUser>();
        var editUserGuid = Guid.NewGuid();
        editUser.UserGuid.Returns(editUserGuid);

        entity.Name = "After";

        var unitOfWork = new FargoUnitOfWork(context, editUser);

        // Act
        await unitOfWork.SaveChanges();

        // Assert
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(creationUserGuid, entity.CreatedByGuid);
        Assert.NotNull(entity.EditedAt);
        Assert.Equal(editUserGuid, entity.EditedByGuid);
    }

    [Fact]
    public async Task SaveChanges_Should_SetCreationAudit_When_AuditedEntityIsAdded()
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
            Name = "Value"
        };

        context.AuditedEntities.Add(entity);

        var unitOfWork = new FargoUnitOfWork(context, currentUser);

        // Act
        var result = await unitOfWork.SaveChanges();

        // Assert
        Assert.True(result > 0);
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(currentUserGuid, entity.CreatedByGuid);
        Assert.Null(entity.EditedAt);
        Assert.Null(entity.EditedByGuid);
    }
}