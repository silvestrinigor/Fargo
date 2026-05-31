using Fargo.Core.Shared.Identity;

namespace Fargo.Core.Tests.ValueObjects;

public sealed class TokenTests
{
    [Fact]
    public void Constructor_Should_CreateToken_When_ValueIsJwtSized()
    {
        var value = new string('a', 1024);

        var token = new Token(value);

        Assert.Equal(value, token.Value);
    }
}
