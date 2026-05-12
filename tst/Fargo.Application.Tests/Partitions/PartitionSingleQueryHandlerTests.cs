using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Application.Tests.Partitions;

public sealed class PartitionSingleQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_AllowDirectPartitionAccess()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var partitionRepository = Substitute.For<IPartitionRepository>();
        var partitionQueryRepository = Substitute.For<IPartitionQueryRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var partition = new Partition { Name = new Name("Direct") };
        var user = new User
        {
            Nameid = new Nameid("actor"),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
        user.AddPartitionAccess(partition);
        currentUser.UserGuid.Returns(user.Guid);
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids => guids.SequenceEqual(new[] { partition.Guid })),
                true,
                Arg.Any<CancellationToken>())
            .Returns([partition.Guid]);
        var dto = new PartitionDto(
            partition.Guid,
            partition.Name,
            partition.Description,
            partition.ParentPartitionGuid,
            partition.IsActive,
            partition.EditedByGuid);
        partitionQueryRepository
            .GetInfoByGuid(
                partition.Guid,
                null,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(dto);
        var sut = new PartitionSingleQueryHandler(
            new ActorService(userRepository, partitionRepository),
            partitionQueryRepository,
            currentUser,
            NullLogger<PartitionSingleQueryHandler>.Instance);

        var result = await sut.Handle(new PartitionSingleQuery(partition.Guid));

        Assert.Equal(partition.Guid, result?.Guid);
    }
}
