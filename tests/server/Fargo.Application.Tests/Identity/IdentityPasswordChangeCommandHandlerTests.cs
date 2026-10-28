using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Identity;

public class IdentityPasswordChangeCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private const string ValidNameid = "jdoe";
    private const string CurrentPassword = "Current#123";

    private static readonly Password NewPassword = new("NewPass#123");

    private IdentityPasswordChangeCommandHandler CreateHandler()
        => new(
            userRepository,
            passwordHasher,
            refreshTokenRepository,
            ActorTestFactory.Resolver(userRepository),
            auditLogRepository,
            Substitute.For<ILogger<IdentityPasswordChangeCommandHandler>>());

    private static IdentityPasswordChangeCommand Command(
        string nameid = ValidNameid, string currentPassword = CurrentPassword)
        => new(new IdentityPasswordUpdateDto(nameid, NewPassword, currentPassword));

    /// <summary>
    /// Builds an active user with a stored password hash and configures the
    /// repository so both nameid lookup and actor resolution return it.
    /// </summary>
    private User ConfigureExistingUser(bool withPasswordHash = true)
    {
        var user = User.CreateUser(new Nameid(ValidNameid));

        if (withPasswordHash)
        {
            user.Authentication.SetPasswordHash(new PasswordHash("stored-hash"));
        }

        userRepository.GetByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        // Actor resolution reads the same active user by its own guid.
        userRepository.GetByGuidAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        userRepository.GetAllActivePermissionsFromUserAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>([]));
        userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        return user;
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdatePasswordRevokeTokensAuditAndSave()
    {
        var user = ConfigureExistingUser();
        passwordHasher.Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>()).Returns(true);
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("new-hash"));

        var activeToken = RefreshToken.Create(user.Guid, new TokenHash(new string('a', 64)));
        refreshTokenRepository.GetByUserGuidAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<RefreshToken>>([activeToken]));

        await CreateHandler().HandleAsync(Command());

        Assert.Equal("new-hash", user.Authentication.PasswordHash!.Value.Value);
        Assert.False(activeToken.IsUsable);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
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
        var user = User.CreateUser(new Nameid(ValidNameid));
        user.Deactivate();
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

    [Fact]
    public async Task HandleAsync_WhenActorCannotBeResolved_ShouldThrowActorNotFound()
    {
        var user = User.CreateUser(new Nameid(ValidNameid));
        user.Authentication.SetPasswordHash(new PasswordHash("stored-hash"));
        userRepository.GetByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        passwordHasher.Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>()).Returns(true);
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("new-hash"));

        // Actor resolution returns null (resolver's GetByGuidAsync not configured
        // to return the user).
        userRepository.GetByGuidAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(Command()));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
