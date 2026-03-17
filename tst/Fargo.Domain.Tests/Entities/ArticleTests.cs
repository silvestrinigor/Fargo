using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class ArticleTests
{
    [Fact]
    public void ObjectInitializer_Should_SetName_And_UseEmptyDescriptionByDefault()
    {
        // Arrange
        var name = new Name("Notebook");

        // Act
        var article = CreateArticle(name: name);

        // Assert
        Assert.Equal(name, article.Name);
        Assert.Equal(Description.Empty, article.Description);
    }

    [Fact]
    public void ObjectInitializer_Should_SetName_And_Description_When_Provided()
    {
        // Arrange
        var name = new Name("Notebook");
        var description = new Description("A portable computer.");

        // Act
        var article = CreateArticle(name, description);

        // Assert
        Assert.Equal(name, article.Name);
        Assert.Equal(description, article.Description);
    }

    [Fact]
    public void Name_Should_BeMutable()
    {
        // Arrange
        var article = CreateArticle();
        var newName = new Name("Keyboard");

        // Act
        article.Name = newName;

        // Assert
        Assert.Equal(newName, article.Name);
    }

    [Fact]
    public void Description_Should_BeMutable()
    {
        // Arrange
        var article = CreateArticle();
        var newDescription = new Description("Mechanical keyboard.");

        // Act
        article.Description = newDescription;

        // Assert
        Assert.Equal(newDescription, article.Description);
    }

    private static Article CreateArticle(
        Name? name = null,
        Description? description = null)
    {
        return new Article
        {
            Name = name ?? new Name("Mouse"),
            Description = description ?? Description.Empty
        };
    }
}
