using Fargo.Domain.Exceptions;

namespace Fargo.Domain.Tests.Exceptions;

public sealed class FargoDomainExceptionTests
{
    private sealed class TestFargoDomainException : FargoDomainException
    {
        public TestFargoDomainException()
        {
        }

        public TestFargoDomainException(string? message)
            : base(message)
        {
        }

        public TestFargoDomainException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    [Fact]
    public void Constructor_Should_CreateException_When_Parameterless()
    {
        // Act
        var exception = new TestFargoDomainException();

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<TestFargoDomainException>(exception);
    }

    [Fact]
    public void Constructor_Should_SetMessage_When_MessageIsProvided()
    {
        // Arrange
        var message = "Domain rule violated.";

        // Act
        var exception = new TestFargoDomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessageAndInnerException_When_BothAreProvided()
    {
        // Arrange
        var message = "Domain rule violated.";
        var innerException = new InvalidOperationException("Inner error.");

        // Act
        var exception = new TestFargoDomainException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Exception_Should_InheritFrom_Exception()
    {
        // Act
        var exception = new TestFargoDomainException();

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}