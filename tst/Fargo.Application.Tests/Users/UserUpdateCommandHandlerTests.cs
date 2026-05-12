using Fargo.Application.Authentication;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Tokens;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public sealed class UserUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_Should_RejectDuplicateNameid_When_NameidChanges()
    {
        var actor = CreateActor(ActionType.EditUser);
        var target = CreateUser("target");
        currentUser.UserGuid.Returns(actor.Guid);
        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>()).Returns(actor);
        userRepository.GetByGuid(target.Guid, Arg.Any<CancellationToken>()).Returns(target);
        userRepository.ExistsByNameid(new Nameid("duplicate"), Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        await Assert.ThrowsAsync<UserNameidAlreadyExistsDomainException>(
            () => sut.Handle(new UserUpdateCommand(
                target.Guid,
                new UserUpdateDto(Nameid: "duplicate"))));
    }

    [Fact]
    public async Task Handle_Should_RejectInactiveUserGroupAssignment()
    {
        var actor = CreateActor(ActionType.EditUser);
        var target = CreateUser("target");
        var inactiveGroup = new UserGroup { Nameid = new Nameid("inactive") };
        inactiveGroup.Deactivate();
        currentUser.UserGuid.Returns(actor.Guid);
        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>()).Returns(actor);
        userRepository.GetByGuid(target.Guid, Arg.Any<CancellationToken>()).Returns(target);
        userGroupRepository.GetByGuid(inactiveGroup.Guid, Arg.Any<CancellationToken>()).Returns(inactiveGroup);
        var sut = CreateSut();

        await Assert.ThrowsAsync<UserGroupInactiveFargoDomainException>(
            () => sut.Handle(new UserUpdateCommand(
                target.Guid,
                new UserUpdateDto(UserGroups: [inactiveGroup.Guid]))));
    }

    private UserUpdateCommandHandler CreateSut()
        => new(
            new ActorService(userRepository, partitionRepository),
            new UserService(userRepository),
            userRepository,
            userGroupRepository,
            partitionRepository,
            passwordHasher,
            refreshTokenRepository,
            unitOfWork,
            currentUser,
            NullLogger<UserUpdateCommandHandler>.Instance);

    private static User CreateActor(ActionType permission)
    {
        var actor = CreateUser("actor");
        actor.AddPermission(permission);
        return actor;
    }

    private static User CreateUser(string nameid)
        => new()
        {
            Nameid = new Nameid(nameid),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
}
