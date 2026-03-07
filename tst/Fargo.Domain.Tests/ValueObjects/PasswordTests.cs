using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects
{
    public sealed class PasswordTests
    {
        [Fact]
        public void Constructor_Should_CreatePassword_When_ValueIsValid()
        {
            // Arrange
            var value = "Secure@123";

            // Act
            var password = new Password(value);

            // Assert
            Assert.Equal(value, password.Value);
        }

        [Fact]
        public void FromString_Should_CreatePassword_When_ValueIsValid()
        {
            // Arrange
            var value = "Secure@123";

            // Act
            var password = Password.FromString(value);

            // Assert
            Assert.Equal(value, password.Value);
        }

        [Fact]
        public void ImplicitOperator_Should_ReturnStringValue()
        {
            // Arrange
            var password = new Password("Secure@123");

            // Act
            string value = password;

            // Assert
            Assert.Equal("Secure@123", value);
        }

        [Fact]
        public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
        {
            // Arrange
            Password password = default;

            // Act
            void act() => _ = password.Value;

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(act);
            Assert.Equal("Password value must be set.", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
        {
            // Act
            void act() => _ = new Password(value!);

            // Assert
            var exception = Assert.Throws<ArgumentException>(act);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
        {
            // Arrange
            var value = "A@1bcdef";

            // Act
            void act() => _ = new Password(value);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
        {
            // Arrange
            var value = new string('A', Password.MaxLength + 1) + "1!";

            // Act
            void act() => _ = new Password(value);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_ValueContainsSpaces()
        {
            // Arrange
            var value = "Secure @123";

            // Act
            void act() => _ = new Password(value);

            // Assert
            var exception = Assert.Throws<ArgumentException>(act);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainLetter()
        {
            // Arrange
            var value = "12345678!";

            // Act
            void act() => _ = new Password(value);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainDigit()
        {
            // Arrange
            var value = "Password!";

            // Act
            void act() => _ = new Password(value);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainSpecialCharacter()
        {
            // Arrange
            var value = "Password1";

            // Act
            void act() => _ = new Password(value);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }
    }
}