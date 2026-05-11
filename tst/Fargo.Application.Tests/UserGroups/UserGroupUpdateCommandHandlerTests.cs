using Fargo.Application.Authentication;
using Fargo.Application.UserGroups;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.UserGroups;
using Fargo.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public sealed class UserGroupUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_RejectDuplicateNameid_When_NameidChanges()
    {
        var actor = CreateActor();
        var target = new UserGroup { Nameid = new Nameid("target") };
        currentUser.UserGuid.Returns(actor.Guid);
        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>()).Returns(actor);
        userGroupRepository.GetByGuid(target.Guid, Arg.Any<CancellationToken>()).Returns(target);
        userGroupRepository.ExistsByNameid(new Nameid("duplicate"), Arg.Any<CancellationToken>()).Returns(true);
        var sut = new UserGroupUpdateCommandHandler(
            new ActorService(userRepository, partitionRepository),
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            partitionRepository,
            unitOfWork,
            currentUser,
            NullLogger<UserGroupUpdateCommandHandler>.Instance);

        await Assert.ThrowsAsync<UserGroupNameidAlreadyExistsDomainException>(
            () => sut.Handle(new UserGroupUpdateCommand(
                target.Guid,
                new UserGroupUpdateDto(
                    Nameid: "duplicate",
                    Description: null,
                    IsActive: null,
                    Permissions: null,
                    Partitions: null))));
    }

    private static User CreateActor()
    {
        var actor = new User
        {
            Nameid = new Nameid("actor"),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
        actor.AddPermission(ActionType.EditUserGroup);
        return actor;
    }
}
