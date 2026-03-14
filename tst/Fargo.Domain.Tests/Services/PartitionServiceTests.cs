using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Domain.Tests.Services;

public sealed class PartitionServiceTests
{
    private readonly IPartitionRepository partitionRepository;
    private readonly PartitionService service;

    public PartitionServiceTests()
    {
        partitionRepository = Substitute.For<IPartitionRepository>();
        service = new PartitionService(partitionRepository);
    }

    [Fact]
    public async Task GetPartition_Should_ReturnNull_When_PartitionDoesNotExist()
    {
        // Arrange
        var partitionGuid = Guid.NewGuid();
        var actor = CreateUser();

        partitionRepository
            .GetByGuid(partitionGuid, Arg.Any<CancellationToken>())
            .Returns((Partition?)null);

        // Act
        var result = await service.GetPartition(partitionGuid, actor);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPartition_Should_ReturnPartition_When_ActorHasAccess()
    {
        // Arrange
        var partition = CreatePartition();
        var actor = CreateUser();

        actor.AddPartitionAccess(partition);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        var result = await service.GetPartition(partition.Guid, actor);

        // Assert
        Assert.Equal(partition, result);
    }

    [Fact]
    public async Task GetPartition_Should_ReturnNull_When_ActorDoesNotHaveAccess()
    {
        // Arrange
        var partition = CreatePartition();
        var actor = CreateUser();

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        var result = await service.GetPartition(partition.Guid, actor);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPartition_Should_CallRepositoryWithProvidedArguments()
    {
        // Arrange
        var partitionGuid = Guid.NewGuid();
        var actor = CreateUser();
        var cancellationToken = new CancellationTokenSource().Token;

        partitionRepository
            .GetByGuid(partitionGuid, cancellationToken)
            .Returns((Partition?)null);

        // Act
        await service.GetPartition(partitionGuid, actor, cancellationToken);

        // Assert
        await partitionRepository.Received(1).GetByGuid(partitionGuid, cancellationToken);
    }

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("testuser"),
            PasswordHash = new PasswordHash(new string('A', PasswordHash.MinLength)),
            PartitionsAccesses = []
        };
    }

    private static Partition CreatePartition()
    {
        return new Partition
        {
            Guid = Guid.NewGuid(),
            Name = new Name(new string('A', Name.MinLength)),
            Description = new Description(new string('A', Description.MinLength)),
            IsActive = true
        };
    }
}