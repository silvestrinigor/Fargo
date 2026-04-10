using Fargo.Sdk;
using Fargo.Sdk.Authentication;

namespace Fargo.Sdk.Tests.Authentication;

public sealed class AuthSessionTests
{
    [Fact]
    public void IsAuthenticated_Should_BeFalse_When_NoTokensSet()
    {
        // Arrange / Act
        var session = new AuthSession();

        // Assert
        Assert.False(session.IsAuthenticated);
    }

    [Fact]
    public void SetTokens_Should_UpdateAllProperties()
    {
        // Arrange
        var session = new AuthSession();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        session.SetTokens("user1", "access-token", "refresh-token", expiresAt, false, [], []);

        // Assert
        Assert.Equal("user1", session.Nameid);
        Assert.Equal("access-token", session.AccessToken);
        Assert.Equal("refresh-token", session.RefreshToken);
        Assert.Equal(expiresAt, session.ExpiresAt);
    }

    [Fact]
    public void IsAuthenticated_Should_BeTrue_After_SetTokens()
    {
        // Arrange
        var session = new AuthSession();

        // Act
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        // Assert
        Assert.True(session.IsAuthenticated);
    }

    [Fact]
    public void Clear_Should_ResetAllProperties()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        // Act
        session.Clear();

        // Assert
        Assert.Null(session.Nameid);
        Assert.Null(session.AccessToken);
        Assert.Null(session.RefreshToken);
        Assert.Null(session.ExpiresAt);
    }

    [Fact]
    public void IsAuthenticated_Should_BeFalse_After_Clear()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        // Act
        session.Clear();

        // Assert
        Assert.False(session.IsAuthenticated);
    }

    [Fact]
    public void IsExpired_Should_BeFalse_When_ExpiresAtIsInFuture()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        // Act / Assert
        Assert.False(session.IsExpired);
    }

    [Fact]
    public void IsExpired_Should_BeTrue_When_ExpiresAtIsInPast()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(-1), false, [], []);

        // Act / Assert
        Assert.True(session.IsExpired);
    }

    [Fact]
    public void HasActionPermission_Should_ReturnFalse_When_NotAuthenticated()
    {
        var session = new AuthSession();

        Assert.False(session.HasActionPermission(ActionType.CreateArticle));
    }

    [Fact]
    public void HasActionPermission_Should_ReturnTrue_When_IsAdmin()
    {
        var session = new AuthSession();
        session.SetTokens("admin", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), true, [], []);

        Assert.True(session.HasActionPermission(ActionType.CreateArticle));
        Assert.True(session.HasActionPermission(ActionType.DeleteUser));
    }

    [Fact]
    public void HasActionPermission_Should_ReturnTrue_When_PermissionGranted()
    {
        var session = new AuthSession();
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [ActionType.EditArticle], []);

        Assert.True(session.HasActionPermission(ActionType.EditArticle));
    }

    [Fact]
    public void HasActionPermission_Should_ReturnFalse_When_PermissionNotGranted()
    {
        var session = new AuthSession();
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [ActionType.EditArticle], []);

        Assert.False(session.HasActionPermission(ActionType.DeleteArticle));
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnFalse_When_NotAuthenticated()
    {
        var session = new AuthSession();

        Assert.False(session.HasPartitionAccess(Guid.NewGuid()));
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnTrue_When_IsAdmin()
    {
        var session = new AuthSession();
        session.SetTokens("admin", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), true, [], []);

        Assert.True(session.HasPartitionAccess(Guid.NewGuid()));
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnTrue_When_PartitionInList()
    {
        var partitionGuid = Guid.NewGuid();
        var session = new AuthSession();
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], [partitionGuid]);

        Assert.True(session.HasPartitionAccess(partitionGuid));
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnFalse_When_PartitionNotInList()
    {
        var session = new AuthSession();
        session.SetTokens("user1", "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], [Guid.NewGuid()]);

        Assert.False(session.HasPartitionAccess(Guid.NewGuid()));
    }

    [Fact]
    public void PermissionActions_Should_ReturnEmpty_When_NotAuthenticated()
    {
        var session = new AuthSession();

        Assert.Empty(session.PermissionActions);
    }

    [Fact]
    public void PartitionAccesses_Should_ReturnEmpty_When_NotAuthenticated()
    {
        var session = new AuthSession();

        Assert.Empty(session.PartitionAccesses);
    }

    [Fact]
    public void SetTokens_Should_OverwritePreviousTokens()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "old-access", "old-refresh", DateTimeOffset.UtcNow.AddHours(1), false, [], []);

        var newExpiry = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        session.SetTokens("user2", "new-access", "new-refresh", newExpiry, false, [], []);

        // Assert
        Assert.Equal("user2", session.Nameid);
        Assert.Equal("new-access", session.AccessToken);
        Assert.Equal("new-refresh", session.RefreshToken);
        Assert.Equal(newExpiry, session.ExpiresAt);
    }
}
