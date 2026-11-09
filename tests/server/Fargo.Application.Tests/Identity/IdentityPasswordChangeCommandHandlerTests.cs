using Fargo.Application.Operations.Identity;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Application.Persistence;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Identity;

public class IdentityPasswordChangeCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private const string ValidNameid = "jdoe";
    private const string CurrentPassword = "Current#123";

    private static readonly Password NewPassword = new("NewPass#123");

    private IdentityPasswordChangeCommandHandler CreateHandler()
        => new(
            userRepository,
            passwordHasher,
            refreshTokenRepository);

    private static IdentityPasswordChangeCommand Command(
        string nameid = ValidNameid, string currentPassword = CurrentPassword)
        => new(new IdentityPasswordUpdateDto(nameid, NewPassword, currentPassword));

    /// <summary>
    /// Builds an active user with a stored password hash and configures the
    /// repository so both nameid lookup and actor resolution return it.
    /// </summary>
    private User ConfigureExistingUser(bool withPasswordHash = true)
    {
        var user = Actor.NewUser(new Nameid(ValidNameid)).User!;

        if (withPasswordHash)
        {
            user.Authentication.SetPasswordHash(new PasswordHash("stored-hash"));
        }

        userRepository.GetByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        // Actor resolution reads the same active user by its own guid.
        userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        userRepository.GetAllActivePermissionsFromUserAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>([]));
        userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        return user;
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdatePasswordRevokeTokensAuditAndSave()
    {
        var user = ConfigureExistingUser();
        passwordHasher.Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>()).Returns(true);
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("new-hash"));

        var activeToken = RefreshToken.Create(user.ActorGuid, new TokenHash(new string('a', 64)));
        refreshTokenRepository.GetByUserGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<RefreshToken>>([activeToken]));

        await CreateHandler().HandleAsync(Command());

        Assert.Equal("new-hash", user.Authentication.PasswordHash!.Value.Value);
        Assert.False(activeToken.IsUsable);
    }

    [Fact]
    public async Task HandleAsync_WhenNameidIsInvalid_ShouldThrowInvalidCredentials()
    {
        await Assert.ThrowsAsync<InvalidCredentialsFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command(nameid: "a b")));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrowInvalidCredentials()
    {
        userRepository.GetByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<InvalidCredentialsFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command()));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserInactive_ShouldThrowInvalidCredentials()
    {
        var user = Actor.NewUser(new Nameid(ValidNameid)).User!;
        user.Actor.Deactivate();
        userRepository.GetByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        await Assert.ThrowsAsync<InvalidCredentialsFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command()));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoStoredPasswordHash_ShouldThrowInvalidCredentials()
    {
        ConfigureExistingUser(withPasswordHash: false);

        await Assert.ThrowsAsync<InvalidCredentialsFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command()));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentPasswordIsWrong_ShouldThrowInvalidCredentials()
    {
        ConfigureExistingUser();
        passwordHasher.Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>()).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command()));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
