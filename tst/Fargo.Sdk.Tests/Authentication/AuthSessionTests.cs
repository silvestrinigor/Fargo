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
        session.SetTokens("user1", "access-token", "refresh-token", expiresAt);

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
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        Assert.True(session.IsAuthenticated);
    }

    [Fact]
    public void Clear_Should_ResetAllProperties()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

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
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

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
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

        // Act / Assert
        Assert.False(session.IsExpired);
    }

    [Fact]
    public void IsExpired_Should_BeTrue_When_ExpiresAtIsInPast()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(-1));

        // Act / Assert
        Assert.True(session.IsExpired);
    }

    [Fact]
    public void SetTokens_Should_OverwritePreviousTokens()
    {
        // Arrange
        var session = new AuthSession();
        session.SetTokens("user1", "old-access", "old-refresh", DateTimeOffset.UtcNow.AddHours(1));

        var newExpiry = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        session.SetTokens("user2", "new-access", "new-refresh", newExpiry);

        // Assert
        Assert.Equal("user2", session.Nameid);
        Assert.Equal("new-access", session.AccessToken);
        Assert.Equal("new-refresh", session.RefreshToken);
        Assert.Equal(newExpiry, session.ExpiresAt);
    }
}
