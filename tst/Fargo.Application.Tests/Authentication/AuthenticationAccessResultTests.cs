using Fargo.Application.Authentication;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Tokens;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Application.Tests.Authentication;

public sealed class AuthenticationAccessResultTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenGenerator refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly ITokenHasher tokenHasher = Substitute.For<ITokenHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Login_Should_ReturnAdminDatabasePermissionsAndExpandedPartitionAccess()
    {
        var globalPartition = CreatePartition(PartitionService.GlobalPartitionGuid, "Global");
        var childPartitionGuid = Guid.NewGuid();
        var admin = CreateAdmin();
        admin.AddPermission(ActionType.CreateArticle);
        admin.AddPartitionAccess(globalPartition);
        userRepository.GetByNameid(admin.Nameid, Arg.Any<CancellationToken>()).Returns(admin);
        userRepository.GetByGuid(admin.Guid, Arg.Any<CancellationToken>()).Returns(admin);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids => guids.SequenceEqual(new[] { globalPartition.Guid })),
                true,
                Arg.Any<CancellationToken>())
            .Returns([globalPartition.Guid, childPartitionGuid]);
        passwordHasher.Verify(admin.PasswordHash, "password").Returns(true);
        tokenGenerator.Generate(admin).Returns(new TokenGenerateResult(CreateToken('a'), DateTimeOffset.UtcNow.AddMinutes(5)));
        refreshTokenGenerator.Generate().Returns(CreateToken('b'));
        tokenHasher.Hash(Arg.Any<Token>()).Returns(CreateTokenHash('c'));
        var sut = new LoginCommandHandler(
            userRepository,
            passwordHasher,
            tokenGenerator,
            refreshTokenGenerator,
            tokenHasher,
            refreshTokenRepository,
            new ActorService(userRepository, partitionRepository),
            unitOfWork,
            NullLogger<LoginCommandHandler>.Instance);

        var result = await sut.Handle(new LoginCommand(admin.Nameid, "password"));

        Assert.True(result.IsAdmin);
        Assert.Equal([ActionType.CreateArticle], result.PermissionActions);
        Assert.Equal(new[] { globalPartition.Guid, childPartitionGuid }.Order(), result.PartitionAccesses.Order());
    }

    [Fact]
    public async Task Refresh_Should_ReturnAdminDatabasePermissionsAndExpandedPartitionAccess()
    {
        var globalPartition = CreatePartition(PartitionService.GlobalPartitionGuid, "Global");
        var childPartitionGuid = Guid.NewGuid();
        var admin = CreateAdmin();
        admin.AddPermission(ActionType.EditUser);
        admin.AddPartitionAccess(globalPartition);
        var oldRefreshToken = CreateToken('o');
        var oldRefreshTokenHash = CreateTokenHash('p');
        var newRefreshToken = CreateToken('n');
        var storedRefreshToken = new RefreshToken
        {
            UserGuid = admin.Guid,
            TokenHash = oldRefreshTokenHash
        };
        tokenHasher.Hash(oldRefreshToken).Returns(oldRefreshTokenHash);
        var newRefreshTokenHash = CreateTokenHash('q');
        tokenHasher.Hash(newRefreshToken).Returns(newRefreshTokenHash);
        refreshTokenRepository.GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(storedRefreshToken);
        userRepository.GetByGuid(admin.Guid, Arg.Any<CancellationToken>()).Returns(admin);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids => guids.SequenceEqual(new[] { globalPartition.Guid })),
                true,
                Arg.Any<CancellationToken>())
            .Returns([globalPartition.Guid, childPartitionGuid]);
        tokenGenerator.Generate(admin).Returns(new TokenGenerateResult(CreateToken('a'), DateTimeOffset.UtcNow.AddMinutes(5)));
        refreshTokenGenerator.Generate().Returns(newRefreshToken);
        var sut = new RefreshCommandHandler(
            userRepository,
            tokenGenerator,
            refreshTokenGenerator,
            tokenHasher,
            refreshTokenRepository,
            new ActorService(userRepository, partitionRepository),
            unitOfWork,
            NullLogger<RefreshCommandHandler>.Instance);

        var result = await sut.Handle(new RefreshCommand(oldRefreshToken));

        Assert.True(result.IsAdmin);
        Assert.Equal([ActionType.EditUser], result.PermissionActions);
        Assert.Equal(new[] { globalPartition.Guid, childPartitionGuid }.Order(), result.PartitionAccesses.Order());
        Assert.Equal(newRefreshTokenHash, storedRefreshToken.ReplacedByTokenHash);
        Assert.NotNull(storedRefreshToken.RevokedAt);
        refreshTokenRepository.Received(1).Add(Arg.Is<RefreshToken>(token =>
            token.UserGuid == admin.Guid &&
            token.TokenHash == newRefreshTokenHash));
    }

    [Fact]
    public async Task Refresh_Should_RevokeOldTokenAndRequirePasswordChange_When_UserMustChangePassword()
    {
        var user = CreateAdmin();
        user.MarkPasswordChangeAsRequired();
        var oldRefreshToken = CreateToken('o');
        var oldRefreshTokenHash = CreateTokenHash('p');
        var storedRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = oldRefreshTokenHash
        };
        tokenHasher.Hash(oldRefreshToken).Returns(oldRefreshTokenHash);
        refreshTokenRepository.GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>()).Returns(storedRefreshToken);
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        var sut = new RefreshCommandHandler(
            userRepository,
            tokenGenerator,
            refreshTokenGenerator,
            tokenHasher,
            refreshTokenRepository,
            new ActorService(userRepository, partitionRepository),
            unitOfWork,
            NullLogger<RefreshCommandHandler>.Instance);

        await Assert.ThrowsAsync<PasswordChangeRequiredFargoApplicationException>(
            () => sut.Handle(new RefreshCommand(oldRefreshToken)));

        Assert.NotNull(storedRefreshToken.RevokedAt);
        refreshTokenRepository.DidNotReceive().Add(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task PasswordChange_Should_RotateAuthVersionAndRevokeUsableRefreshTokens()
    {
        var user = CreateAdmin();
        var previousAuthVersion = user.AuthVersion;
        var currentRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = CreateTokenHash('r')
        };
        currentUser.UserGuid.Returns(user.Guid);
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.Verify(user.PasswordHash, "old-password").Returns(true);
        passwordHasher.Hash("New-password1!").Returns(CreatePasswordHash());
        refreshTokenRepository.GetByUserGuid(user.Guid, Arg.Any<CancellationToken>()).Returns([currentRefreshToken]);
        var sut = new PasswordChangeCommandHandler(
            userRepository,
            passwordHasher,
            refreshTokenRepository,
            unitOfWork,
            currentUser,
            NullLogger<PasswordChangeCommandHandler>.Instance);

        await sut.Handle(new PasswordChangeCommand(new UserPasswordUpdateDto(
            "New-password1!",
            "old-password")));

        Assert.NotEqual(previousAuthVersion, user.AuthVersion);
        Assert.NotNull(currentRefreshToken.RevokedAt);
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_Should_RevokeRefreshToken()
    {
        var user = CreateAdmin();
        var refreshToken = CreateToken('l');
        var refreshTokenHash = CreateTokenHash('m');
        var storedRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = refreshTokenHash
        };
        tokenHasher.Hash(refreshToken).Returns(refreshTokenHash);
        refreshTokenRepository.GetByTokenHash(refreshTokenHash, Arg.Any<CancellationToken>()).Returns(storedRefreshToken);
        var sut = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork,
            NullLogger<LogoutCommandHandler>.Instance);

        await sut.Handle(new LogoutCommand(refreshToken));

        Assert.NotNull(storedRefreshToken.RevokedAt);
        refreshTokenRepository.DidNotReceive().Remove(Arg.Any<RefreshToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    private static User CreateAdmin()
        => new()
        {
            Guid = UserService.DefaultAdministratorUserGuid,
            Nameid = new Nameid("admin"),
            PasswordHash = CreatePasswordHash()
        };

    private static Partition CreatePartition(Guid guid, string name)
        => new()
        {
            Guid = guid,
            Name = new Name(name)
        };

    private static PasswordHash CreatePasswordHash()
        => new(new string('h', PasswordHash.MinLength));

    private static Token CreateToken(char value)
        => new(new string(value, Token.MinLength));

    private static TokenHash CreateTokenHash(char value)
        => new(new string(value, TokenHash.MinLength));
}
