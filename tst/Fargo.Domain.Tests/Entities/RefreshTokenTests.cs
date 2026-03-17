using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void DefaultExpirationTimeSpan_Should_Be10Days()
    {
        // Assert
        Assert.Equal(TimeSpan.FromDays(10), RefreshToken.DefaultExpirationTimeSpan);
    }

    [Fact]
    public void ExpiresAt_Should_DefaultTo_UtcNowPlusDefaultExpirationTimeSpan()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var refreshToken = CreateRefreshToken();

        var after = DateTimeOffset.UtcNow;

        // Assert
        var expectedMin = before + RefreshToken.DefaultExpirationTimeSpan;
        var expectedMax = after + RefreshToken.DefaultExpirationTimeSpan;

        Assert.InRange(refreshToken.ExpiresAt, expectedMin, expectedMax);
    }

    [Fact]
    public void ReplacedByTokenHash_Should_DefaultTo_Null()
    {
        // Act
        var refreshToken = CreateRefreshToken();

        // Assert
        Assert.Null(refreshToken.ReplacedByTokenHash);
    }

    [Fact]
    public void IsExpired_Should_ReturnFalse_When_ExpiresAtIsInFuture()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(1));

        // Act
        var result = refreshToken.IsExpired;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_Should_ReturnTrue_When_ExpiresAtIsInPast()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        var result = refreshToken.IsExpired;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExpired_Should_ReturnTrue_When_ExpiresAtIsNowOrEarlier()
    {
        // Arrange
        var refreshToken = CreateRefreshToken(
            expiresAt: DateTimeOffset.UtcNow);

        // Act
        var result = refreshToken.IsExpired;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Should_Assign_UserGuid()
    {
        // Arrange
        var userGuid = Guid.NewGuid();

        // Act
        var refreshToken = CreateRefreshToken(userGuid: userGuid);

        // Assert
        Assert.Equal(userGuid, refreshToken.UserGuid);
    }

    [Fact]
    public void Should_Assign_TokenHash()
    {
        // Arrange
        var tokenHash = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var refreshToken = CreateRefreshToken(tokenHash: tokenHash);

        // Assert
        Assert.Equal(tokenHash, refreshToken.TokenHash);
    }

    [Fact]
    public void Should_Assign_ReplacedByTokenHash_When_Provided()
    {
        // Arrange
        var replacedByTokenHash = new TokenHash(new string('b', TokenHash.MinLength));

        // Act
        var refreshToken = CreateRefreshToken(replacedByTokenHash: replacedByTokenHash);

        // Assert
        Assert.Equal(replacedByTokenHash, refreshToken.ReplacedByTokenHash);
    }

    [Fact]
    public void Should_InheritGuidFromEntity()
    {
        // Act
        var refreshToken = CreateRefreshToken();

        // Assert
        Assert.NotEqual(Guid.Empty, refreshToken.Guid);
    }

    private static RefreshToken CreateRefreshToken(
        Guid? userGuid = null,
        TokenHash? tokenHash = null,
        DateTimeOffset? expiresAt = null,
        TokenHash? replacedByTokenHash = null)
    {
        return new RefreshToken
        {
            UserGuid = userGuid ?? Guid.NewGuid(),
            TokenHash = tokenHash ?? new TokenHash(new string('a', TokenHash.MinLength)),
            ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(10),
            ReplacedByTokenHash = replacedByTokenHash
        };
    }
}
