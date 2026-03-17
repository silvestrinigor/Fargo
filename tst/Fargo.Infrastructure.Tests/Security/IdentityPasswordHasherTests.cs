using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Security;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class IdentityPasswordHasherTests
{
    private readonly IdentityPasswordHasher hasher = new();

    [Fact]
    public void Hash_Should_ReturnPasswordHash()
    {
        // Arrange
        var password = new Password("MySecurePassword123!");

        // Act
        var result = hasher.Hash(password);

        // Assert
        Assert.IsType<PasswordHash>(result);
    }

    [Fact]
    public void Hash_Should_ReturnNonEmptyValue()
    {
        // Arrange
        var password = new Password("MySecurePassword123!");

        // Act
        var result = hasher.Hash(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Value));
    }

    [Fact]
    public void Hash_Should_ReturnDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = new Password("MySecurePassword123!");

        // Act
        var first = hasher.Hash(password);
        var second = hasher.Hash(password);

        // Assert
        Assert.NotEqual(first.Value, second.Value);
    }

    [Fact]
    public void Verify_Should_ReturnTrue_When_PasswordMatchesHash()
    {
        // Arrange
        var password = new Password("MySecurePassword123!");
        var hash = hasher.Hash(password);

        // Act
        var result = hasher.Verify(hash, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_Should_ReturnFalse_When_PasswordDoesNotMatchHash()
    {
        // Arrange
        var correctPassword = new Password("MySecurePassword123!");
        var wrongPassword = new Password("WrongPassword123!");
        var hash = hasher.Hash(correctPassword);

        // Act
        var result = hasher.Verify(hash, wrongPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_Should_ReturnTrue_ForHashGeneratedByHashMethod()
    {
        // Arrange
        var password = new Password("AnotherPassword456@");
        var hash = hasher.Hash(password);

        // Act
        var result = hasher.Verify(hash, password);

        // Assert
        Assert.True(result);
    }
}
