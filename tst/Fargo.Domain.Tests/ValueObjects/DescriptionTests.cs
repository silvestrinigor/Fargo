using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class DescriptionTests
{
    [Fact]
    public void Constructor_Should_CreateDescription_When_ValueIsValid()
    {
        // Arrange
        var value = DescriptionFakes.ValidString();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_CreateDescription_When_ValueIsEmpty()
    {
        // Arrange
        var value = DescriptionFakes.EmptyString();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }

    [Fact]
    public void Constructor_Should_CreateDescription_When_ValueHasMaxLength()
    {
        // Arrange
        var value = DescriptionFakes.MaxLengthString();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_CreateDescription_WithLongMultilingualText()
    {
        // Arrange
        var value = DescriptionFakes.LongMultilingualText();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_CreateDescription_WithAccentedText()
    {
        // Arrange
        var value = DescriptionFakes.AccentedLatinText();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_CreateDescription_WithChineseText()
    {
        // Arrange
        var value = DescriptionFakes.ChineseText();

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_ValueIsNull()
    {
        // Arrange
        string? value = null;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => new Description(value!));

        // Assert
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = DescriptionFakes.TooLongString();

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Description(value));

        // Assert
        Assert.Equal("value", exception.ParamName);
        Assert.Contains(
            $"Description length must be between {Description.MinLength} and {Description.MaxLength} characters.",
            exception.Message
        );
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DescriptionIsDefault()
    {
        // Arrange
        var description = default(Description);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => _ = description.Value);

        // Assert
        Assert.Equal("Description not initialized.", exception.Message);
    }

    [Fact]
    public void Empty_Should_Return_EmptyDescription()
    {
        // Act
        var description = Description.Empty;

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }

    [Fact]
    public void FromString_Should_Return_ValidDescription()
    {
        // Arrange
        var value = DescriptionFakes.ValidString();

        // Act
        var description = Description.FromString(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void ToString_Should_Return_UnderlyingValue()
    {
        // Arrange
        var value = DescriptionFakes.ValidString();
        var description = new Description(value);

        // Act
        var result = description.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var value = DescriptionFakes.ValidString();
        var left = new Description(value);
        var right = new Description(value);

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Description("Description A");
        var right = new Description("Description B");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_Should_ReturnTrue_When_ObjectIsEquivalentDescription()
    {
        // Arrange
        object obj = new Description("Same value");
        var description = new Description("Same value");

        // Act
        var result = description.Equals(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Object_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var description = DescriptionFakes.Valid();

        // Act
        var result = description.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var description = DescriptionFakes.Valid();

        // Act
        var result = description.Equals("Valid description");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHashCode_ForEqualDescriptions()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var leftHashCode = left.GetHashCode();
        var rightHashCode = right.GetHashCode();

        // Assert
        Assert.Equal(leftHashCode, rightHashCode);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_DescriptionsAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_DescriptionsAreDifferent()
    {
        // Arrange
        var left = new Description("Description A");
        var right = new Description("Description B");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_DescriptionsAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_DescriptionsAreDifferent()
    {
        // Arrange
        var left = new Description("Description A");
        var right = new Description("Description B");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ImplicitOperator_Should_ConvertDescriptionToString()
    {
        // Arrange
        var description = new Description("Implicit conversion");

        // Act
        string value = description;

        // Assert
        Assert.Equal("Implicit conversion", value);
    }

    [Fact]
    public void ExplicitOperator_Should_ConvertStringToDescription()
    {
        // Arrange
        var value = "Explicit conversion";

        // Act
        var description = (Description)value;

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void DefaultDescriptions_Should_BeEqual()
    {
        // Arrange
        var left = default(Description);
        var right = default(Description);

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DefaultDescription_Should_NotBeEqual_ToInitializedDescription()
    {
        // Arrange
        var left = default(Description);
        var right = new Description("Initialized");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new Description("description");
        var right = new Description("Description");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new Description("description");
        var right = new Description("Description");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new Description("description");
        var right = new Description("Description");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_Should_BeDifferent_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new Description("description");
        var right = new Description("Description");

        // Act
        var leftHashCode = left.GetHashCode();
        var rightHashCode = right.GetHashCode();

        // Assert
        Assert.NotEqual(leftHashCode, rightHashCode);
    }
}

public static class DescriptionFakes
{
    public static Description Valid(
        string? value = null
        )
        => new(value ?? "Valid description");

    public static string ValidString(
        string? value = null
        )
        => value ?? "Valid description";

    public static string EmptyString()
        => string.Empty;

    public static string TooLongString()
        => new('a', Description.MaxLength + 1);

    public static string MaxLengthString()
        => new('a', Description.MaxLength);

    public static string RandomWithLength(
        int length
        )
        => new('a', length);

    public static string AccentedLatinText()
        => "Descrição com acentuação: ação, coração, informação, café, maçã, " +
           "São Paulo, naïve, façade, über, crème brûlée, niño, jalapeño.";

    public static string ChineseText()
        => "这是一个用于测试的中文描述，包含简体字、标点符号，以及不同长度的文本内容。";

    public static string JapaneseText()
        => "これはテスト用の日本語の説明文です。句読点や長めの文章も含まれています。";

    public static string ArabicText()
        => "هذا وصف تجريبي باللغة العربية يحتوي على أحرف خاصة وعلامات ترقيم ونص أطول قليلاً.";

    public static string MixedLanguageText()
        => "Português: descrição válida. English: valid description. 中文：有效描述。 " +
           "日本語: 有効な説明。 العربية: وصف صالح.";

    public static string LongMultilingualText()
        => string.Join(
            " ",
            [
                "Descrição longa com acentuação e cedilha: organização, atualização, eficiência, São José, útil, histórico.",
                "English section with punctuation, numbers 12345, and symbols like % & / ( ) [ ].",
                "Texto em español con acentos: información, administración, operación, país, corazón.",
                "Texte en français avec accents : édition, façade, élève, rôle, coopération.",
                "Deutscher Text mit Umlauten: Einführung, Überprüfung, Möglichkeit, Größe, Änderungen.",
                "中文段落：这是一个较长的描述文本，用于验证系统是否正确处理多字节字符和不同语言内容。",
                "日本語の段落：この長い説明文は、システムがマルチバイト文字を正しく扱えるかを確認するためのものです。"
            ]
        );

    public static string MaxLengthMultilingualString()
    {
        const string seed =
            "Descrição válida com acentos, 中文内容, 日本語テキスト, العربية, English text. ";

        var result = seed;

        while (result.Length + seed.Length <= Description.MaxLength)
        {
            result += seed;
        }

        if (result.Length < Description.MaxLength)
        {
            result += new string('x', Description.MaxLength - result.Length);
        }

        return result;
    }
}