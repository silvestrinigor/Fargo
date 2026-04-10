using Fargo.Sdk.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Authentication;

public sealed class AuthenticationManagerTests
{
    private readonly IAuthenticationClient client = Substitute.For<IAuthenticationClient>();
    private readonly AuthSession session = new();
    private readonly ISessionStore store = Substitute.For<ISessionStore>();
    private readonly AuthenticationManager sut;

    public AuthenticationManagerTests()
    {
        sut = new AuthenticationManager(client, session, NullLogger.Instance, store);
    }

    // --- LogInAsync ---

    [Fact]
    public async Task LogInAsync_Should_SetSession_When_CredentialsAreValid()
    {
        // Arrange
        var authResult = Fakes.AuthResult();
        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(authResult));

        // Act
        await sut.LogInAsync("user1", "password");

        // Assert
        Assert.True(sut.IsAuthenticated);
        Assert.Equal("user1", session.Nameid);
    }

    [Fact]
    public async Task LogInAsync_Should_SaveToStore_When_CredentialsAreValid()
    {
        // Arrange
        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        // Act
        await sut.LogInAsync("user1", "password");

        // Assert
        await store.Received(1).SaveAsync(Arg.Any<StoredSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogInAsync_Should_RaiseLoggedInEvent_When_CredentialsAreValid()
    {
        // Arrange
        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        LoggedInEventArgs? raisedArgs = null;
        sut.LoggedIn += (_, args) => raisedArgs = args;

        // Act
        await sut.LogInAsync("user1", "password");

        // Assert
        Assert.NotNull(raisedArgs);
        Assert.Equal("user1", raisedArgs.Nameid);
    }

    [Fact]
    public async Task LogInAsync_Should_ThrowInvalidCredentials_When_PasswordIsWrong()
    {
        // Arrange
        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(new FargoSdkError(FargoSdkErrorType.InvalidCredentials, "Invalid password.")));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidCredentialsFargoSdkException>(() =>
            sut.LogInAsync("user1", "wrong"));
    }

    [Fact]
    public async Task LogInAsync_Should_ThrowPasswordChangeRequired_When_PasswordMustChange()
    {
        // Arrange
        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(new FargoSdkError(FargoSdkErrorType.PasswordChangeRequired, "Password change required.")));

        // Act / Assert
        await Assert.ThrowsAsync<PasswordChangeRequiredFargoSdkException>(() =>
            sut.LogInAsync("user1", "pass"));
    }

    [Fact]
    public async Task LogInAsync_Should_LogOutFirst_When_AlreadyAuthenticated()
    {
        // Arrange
        session.SetTokens("old-user", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        client.LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        client.LogInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        // Act
        await sut.LogInAsync("new-user", "password");

        // Assert
        await client.Received(1).LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // --- LogOutAsync ---

    [Fact]
    public async Task LogOutAsync_Should_ClearSession_When_Authenticated()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        // Act
        await sut.LogOutAsync();

        // Assert
        Assert.False(sut.IsAuthenticated);
    }

    [Fact]
    public async Task LogOutAsync_Should_ClearStore_When_Authenticated()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        // Act
        await sut.LogOutAsync();

        // Assert
        await store.Received(1).ClearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogOutAsync_Should_RaiseLoggedOutEvent_When_Authenticated()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        LoggedOutEventArgs? raisedArgs = null;
        sut.LoggedOut += (_, args) => raisedArgs = args;

        // Act
        await sut.LogOutAsync();

        // Assert
        Assert.NotNull(raisedArgs);
        Assert.Equal("user1", raisedArgs.Nameid);
    }

    [Fact]
    public async Task LogOutAsync_Should_DoNothing_When_NotAuthenticated()
    {
        // Act
        await sut.LogOutAsync();

        // Assert
        await client.DidNotReceive().LogOutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // --- RefreshAsync ---

    [Fact]
    public async Task RefreshAsync_Should_ThrowUserNotAuthenticated_When_NotLoggedIn()
    {
        // Act / Assert
        await Assert.ThrowsAsync<UserNotAuthenticatedFargoSdkException>(() =>
            sut.RefreshAsync());
    }

    [Fact]
    public async Task RefreshAsync_Should_UpdateSession_When_TokenIsValid()
    {
        // Arrange
        session.SetTokens("user1", "old-access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        var newResult = new AuthResult("new-access", "new-refresh", DateTimeOffset.UtcNow.AddHours(2), false, [], []);
        client.Refresh(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(newResult));

        // Act
        await sut.RefreshAsync();

        // Assert
        Assert.Equal("new-access", session.AccessToken);
        Assert.Equal("new-refresh", session.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_Should_SaveToStore_When_TokenIsValid()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.Refresh(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        // Act
        await sut.RefreshAsync();

        // Assert
        await store.Received(1).SaveAsync(Arg.Any<StoredSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_Should_RaiseRefreshedEvent_When_TokenIsValid()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.Refresh(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        RefreshedEventArgs? raisedArgs = null;
        sut.Refreshed += (_, args) => raisedArgs = args;

        // Act
        await sut.RefreshAsync();

        // Assert
        Assert.NotNull(raisedArgs);
        Assert.Equal("user1", raisedArgs.Nameid);
    }

    // --- ChangePasswordAsync ---

    [Fact]
    public async Task ChangePasswordAsync_Should_ThrowUserNotAuthenticated_When_NotLoggedIn()
    {
        // Act / Assert
        await Assert.ThrowsAsync<UserNotAuthenticatedFargoSdkException>(() =>
            sut.ChangePasswordAsync("new-pass", "old-pass"));
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_ThrowInvalidCredentials_When_CurrentPasswordIsWrong()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.ChangePassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidCredentials, "Invalid password.")));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidCredentialsFargoSdkException>(() =>
            sut.ChangePasswordAsync("new-pass", "wrong-current"));
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_RaisePasswordChangedEvent_When_Successful()
    {
        // Arrange
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        client.ChangePassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        PasswordChangedEventArgs? raisedArgs = null;
        sut.PasswordChanged += (_, args) => raisedArgs = args;

        // Act
        await sut.ChangePasswordAsync("new-pass", "old-pass");

        // Assert
        Assert.NotNull(raisedArgs);
        Assert.Equal("user1", raisedArgs.Nameid);
    }

    // --- RestoreAsync ---

    [Fact]
    public async Task RestoreAsync_Should_ReturnFalse_When_NoStoreConfigured()
    {
        // Arrange
        var sutWithoutStore = new AuthenticationManager(client, session, NullLogger.Instance);

        // Act
        var result = await sutWithoutStore.RestoreAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreAsync_Should_ReturnFalse_When_NoSavedSession()
    {
        // Arrange
        store.LoadAsync(Arg.Any<CancellationToken>()).Returns((StoredSession?)null);

        // Act
        var result = await sut.RestoreAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreAsync_Should_ReturnTrue_And_SetSession_When_SessionExists()
    {
        // Arrange
        var stored = new StoredSession("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
        store.LoadAsync(Arg.Any<CancellationToken>()).Returns(stored);

        // Act
        var result = await sut.RestoreAsync();

        // Assert
        Assert.True(result);
        Assert.True(sut.IsAuthenticated);
        Assert.Equal("user1", session.Nameid);
    }

    [Fact]
    public async Task RestoreAsync_Should_RefreshToken_When_StoredSessionIsExpired()
    {
        // Arrange
        var stored = new StoredSession("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(-1), false, [], []);
        store.LoadAsync(Arg.Any<CancellationToken>()).Returns(stored);

        client.Refresh(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<AuthResult>(Fakes.AuthResult()));

        // Act
        await sut.RestoreAsync();

        // Assert
        await client.Received(1).Refresh(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static class Fakes
    {
        public static AuthResult AuthResult() =>
            new("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1), false, [], []);
    }
}
