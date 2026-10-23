using Fargo.Core.Security;

namespace Fargo.Core.Tests.Security;

public class PasswordTests
{
    [Fact]
    public void Constructor_WhenValid_ShouldSetValue()
    {
        var password = new Password("Str0ng!pass");

        Assert.Equal("Str0ng!pass", password.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenNullEmptyOrWhitespace_ShouldThrowArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Password(value));
    }

    [Fact]
    public void Constructor_WhenTooShort_ShouldThrowArgumentOutOfRange()
    {
        // 8 chars, below the 9 minimum.
        Assert.Throws<ArgumentOutOfRangeException>(() => new Password("Ab1!efgh"));
    }

    [Fact]
    public void Constructor_WhenTooLong_ShouldThrowArgumentOutOfRange()
    {
        var tooLong = "A1!" + new string('a', Password.MaxLength);

        Assert.Throws<ArgumentOutOfRangeException>(() => new Password(tooLong));
    }

    [Fact]
    public void Constructor_WhenContainsSpace_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Password("Str0ng ! pass"));
    }

    [Fact]
    public void Constructor_WhenNoLetter_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Password("1234567!8"));
    }

    [Fact]
    public void Constructor_WhenNoDigit_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Password("Password!!"));
    }

    [Fact]
    public void Constructor_WhenNoSpecialCharacter_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Password("Password12"));
    }

    [Fact]
    public void Value_WhenDefaultStruct_ShouldThrowInvalidOperation()
    {
        var uninitialized = default(Password);

        Assert.Throws<InvalidOperationException>(() => uninitialized.Value);
    }

    [Fact]
    public void Equality_WhenSameValue_ShouldBeEqual()
    {
        var a = new Password("Str0ng!pass");
        var b = new Password("Str0ng!pass");

        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_WhenDifferentValue_ShouldNotBeEqual()
    {
        var a = new Password("Str0ng!pass");
        var b = new Password("0ther!pass1");

        Assert.True(a != b);
    }
}
