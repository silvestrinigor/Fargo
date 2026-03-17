using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Security;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class Sha256TokenHasherTests
{
    private readonly Sha256TokenHasher hasher = new();

    [Fact]
    public void Hash_Should_ReturnTokenHash()
    {
        // Arrange
        var token = CreateValidTestToken();

        // Act
        var result = hasher.Hash(token);

        // Assert
        Assert.IsType<TokenHash>(result);
    }

    [Fact]
    public void Hash_Should_ReturnDeterministicHash_ForSameToken()
    {
        // Arrange
        var token = CreateValidTestToken();

        // Act
        var first = hasher.Hash(token);
        var second = hasher.Hash(token);

        // Assert
        Assert.Equal(first.Value, second.Value);
    }

    [Fact]
    public void Hash_Should_ReturnDifferentHash_ForDifferentTokens()
    {
        // Arrange
        var token1 = CreateValidTestToken();
        var token2 = new Token("token-twoafsdfasfahsdfjsahfdjsahdfjsahfsajdhfsafhsadjfaksdfjsadfjasdjhfsajdfhjdsa");

        // Act
        var hash1 = hasher.Hash(token1);
        var hash2 = hasher.Hash(token2);

        // Assert
        Assert.NotEqual(hash1.Value, hash2.Value);
    }

    [Fact]
    public void Hash_Should_Return64CharacterHexString()
    {
        // Arrange
        var token = CreateValidTestToken();

        // Act
        var result = hasher.Hash(token);

        // Assert
        Assert.Equal(64, result.Value.Length);
    }

    [Fact]
    public void Hash_Should_ReturnUppercaseHex()
    {
        // Arrange
        var token = CreateValidTestToken();

        // Act
        var result = hasher.Hash(token);

        // Assert
        Assert.All(result.Value, c =>
            Assert.True(
                char.IsDigit(c) || (c >= 'A' && c <= 'F'),
                "Hash contains invalid hex character"
            ));
    }

    [Fact]
    public void Hash_Should_MatchExpectedSha256()
    {
        // Arrange
        var tokenValue = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var token = new Token(tokenValue);

        // Known SHA256 of the token above
        var expected = new TokenHash("61dc0bd928eb71b3139935434b974b24d16e951efd747d8908e2654cf7fed4db");

        // Act
        var result = hasher.Hash(token);

        // Assert
        Assert.Equal(expected, result);
    }

    private static Token CreateValidTestToken()
    {
        return new Token("testtokenaflsaffasfhkjsafhsadjfhaskfhsakfjsadkfhasfhiewuudhfsakjahfaskdfhsahfsa");
    }
}
