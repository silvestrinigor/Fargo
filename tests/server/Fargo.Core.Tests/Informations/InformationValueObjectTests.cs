using Fargo.Core.Informations;

namespace Fargo.Core.Tests.Informations;

public class NameTests
{
    [Fact]
    public void Constructor_WhenValid_ShouldSetValue()
    {
        var name = new Name("Widget");

        Assert.Equal("Widget", name.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenNullEmptyOrWhitespace_ShouldThrowArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Name(value));
    }

    [Fact]
    public void Constructor_WhenTooLong_ShouldThrowArgumentOutOfRange()
    {
        var tooLong = new string('a', Name.MaxLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new Name(tooLong));
    }

    [Fact]
    public void Equality_IsOrdinalCaseSensitive()
    {
        Assert.Equal(new Name("Box"), new Name("Box"));
        Assert.NotEqual(new Name("Box"), new Name("box"));
    }

    [Fact]
    public void TryParse_WhenInvalid_ShouldReturnFalse()
    {
        var ok = Name.TryParse("", null, out _);

        Assert.False(ok);
    }

    [Fact]
    public void TryParse_WhenValid_ShouldReturnTrue()
    {
        var ok = Name.TryParse("Widget", null, out var name);

        Assert.True(ok);
        Assert.Equal("Widget", name.Value);
    }

    [Fact]
    public void ImplicitStringConversion_ShouldReturnValue()
    {
        string value = new Name("Widget");

        Assert.Equal("Widget", value);
    }
}

public class NameidTests
{
    [Fact]
    public void Constructor_WhenValid_ShouldLowercaseValue()
    {
        var nameid = new Nameid("Admin");

        Assert.Equal("admin", nameid.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_WhenNullEmptyOrWhitespace_ShouldThrowArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Nameid(value));
    }

    [Fact]
    public void Constructor_WhenTooShort_ShouldThrowArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nameid("ab"));
    }

    [Theory]
    [InlineData("has space")]     // spaces not allowed
    [InlineData(".leading")]      // must start with letter/digit
    [InlineData("trailing.")]     // must end with letter/digit
    [InlineData("a..b")]          // consecutive separators
    [InlineData("a@b")]           // disallowed character
    public void Constructor_WhenMalformed_ShouldThrowArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Nameid(value));
    }

    [Theory]
    [InlineData("john.doe")]
    [InlineData("user_1")]
    [InlineData("a-b-c")]
    [InlineData("abc")]
    public void Constructor_WhenValidFormats_ShouldSucceed(string value)
    {
        var nameid = new Nameid(value);

        Assert.Equal(value.ToLowerInvariant(), nameid.Value);
    }

    [Fact]
    public void Equality_IsCaseInsensitive()
    {
        Assert.Equal(new Nameid("Admin"), new Nameid("admin"));
        Assert.Equal(new Nameid("ADMIN").GetHashCode(), new Nameid("admin").GetHashCode());
    }
}
