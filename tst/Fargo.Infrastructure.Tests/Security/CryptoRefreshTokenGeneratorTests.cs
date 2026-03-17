using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Security;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class CryptoRefreshTokenGeneratorTests
{
    private readonly CryptoRefreshTokenGenerator generator = new();

    [Fact]
    public void Generate_Should_ReturnToken()
    {
        // Act
        var result = generator.Generate();

        // Assert
        Assert.IsType<Token>(result);
    }

    [Fact]
    public void Generate_Should_ReturnTokenWithNonEmptyValue()
    {
        // Act
        var result = generator.Generate();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Value));
    }

    [Fact]
    public void Generate_Should_ReturnValidBase64String()
    {
        // Act
        var result = generator.Generate();

        // Assert
        var exception = Record.Exception(() => Convert.FromBase64String(result.Value));
        Assert.Null(exception);
    }

    [Fact]
    public void Generate_Should_ReturnBase64StringRepresenting64Bytes()
    {
        // Act
        var result = generator.Generate();
        var bytes = Convert.FromBase64String(result.Value);

        // Assert
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void Generate_Should_ReturnDifferentTokensOnConsecutiveCalls()
    {
        // Act
        var first = generator.Generate();
        var second = generator.Generate();

        // Assert
        Assert.NotEqual(first.Value, second.Value);
    }
}
