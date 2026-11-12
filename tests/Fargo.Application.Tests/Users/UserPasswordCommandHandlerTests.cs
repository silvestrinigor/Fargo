using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.Users;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserPasswordCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly TestCurrentActor currentActor = new();

    public UserPasswordCommandHandlerTests()
    {
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("new-hash"));
    }

    private UserResetPasswordCommandHandler CreateResetHandler(ICurrentActor? actor = null)
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            passwordHasher,
            actor ?? currentActor,
            TimeProvider.System);

    private UserSetPasswordExpirationCommandHandler CreateSetExpirationHandler()
        => new(ActorTestFactory.Resolver(userRepository), userRepository, currentActor);

    private UserRemovePasswordExpirationCommandHandler CreateRemoveExpirationHandler()
        => new(ActorTestFactory.Resolver(userRepository), userRepository, currentActor);

    private static User NewTarget()
        => Actor.NewUser(new Nameid("target")).User!;

    private void Existing(User user)
        => userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

    private static UserResetPasswordCommand Reset(User user)
        => new(user.ActorGuid, new PasswordResetDto(new Password("Temp-Secret-1234")));

    [Fact]
    public async Task Reset_WhenValid_ShouldSetTheHashAndRequireAPasswordChange()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var target = NewTarget();
        Existing(target);

        await CreateResetHandler().HandleAsync(Reset(target));

        Assert.Equal("new-hash", target.Authentication.PasswordHash?.Value);
        Assert.NotNull(target.Authentication.RequirePasswordChangeAt);
    }

    [Fact]
    public async Task Reset_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUser]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateResetHandler().HandleAsync(Reset(target)));

        Assert.Null(target.Authentication.PasswordHash);
    }

    [Fact]
    public async Task Reset_WhenActorResetsItsOwnPassword_ShouldThrow()
    {
        var self = NewTarget();
        ActorTestFactory.ConfigureUserActor(userRepository, self.ActorGuid, [ActionType.ChangeAnotherUserPassword]);
        Existing(self);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateResetHandler(new TestCurrentActor { Guid = self.ActorGuid }).HandleAsync(Reset(self)));

        Assert.Null(self.Authentication.PasswordHash);
    }

    [Fact]
    public async Task Reset_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateResetHandler().HandleAsync(
                new UserResetPasswordCommand(missing, new PasswordResetDto(new Password("Temp-Secret-1234")))));
    }

    [Fact]
    public async Task Reset_WhenActorHasNoAccessToTheUser_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword], partitionAccesses: [Guid.NewGuid()]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateResetHandler().HandleAsync(Reset(target)));
    }

    [Fact]
    public async Task SetExpiration_WhenValid_ShouldSetThePeriod()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var target = NewTarget();
        Existing(target);

        await CreateSetExpirationHandler().HandleAsync(
            new UserSetPasswordExpirationCommand(target.ActorGuid, new PasswordExpirationDto(TimeSpan.FromDays(90))));

        Assert.Equal(TimeSpan.FromDays(90), target.Authentication.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public async Task SetExpiration_WhenPeriodIsNegative_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateSetExpirationHandler().HandleAsync(
                new UserSetPasswordExpirationCommand(target.ActorGuid, new PasswordExpirationDto(TimeSpan.FromDays(-1)))));

        Assert.Null(target.Authentication.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public async Task SetExpiration_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUser]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateSetExpirationHandler().HandleAsync(
                new UserSetPasswordExpirationCommand(target.ActorGuid, new PasswordExpirationDto(TimeSpan.FromDays(90)))));
    }

    [Fact]
    public async Task RemoveExpiration_WhenValid_ShouldClearThePeriod()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var target = NewTarget();
        target.Authentication.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(30);
        Existing(target);

        await CreateRemoveExpirationHandler().HandleAsync(new UserRemovePasswordExpirationCommand(target.ActorGuid));

        Assert.Null(target.Authentication.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public async Task RemoveExpiration_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeAnotherUserPassword]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateRemoveExpirationHandler().HandleAsync(new UserRemovePasswordExpirationCommand(missing)));
    }
}
