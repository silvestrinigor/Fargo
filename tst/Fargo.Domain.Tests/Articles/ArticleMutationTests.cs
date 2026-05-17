using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Tests.Articles;

public sealed class ArticleMutationTests
{
    [Fact]
    public void CreateArticle_Should_NotSetActorOrModificationType()
    {
        var article = Article.CreateArticle(new Name("Test article"));

        Assert.Null(article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.None, article.ModificationTypes);
        Assert.False(article.IsEditStarted);
    }

    [Fact]
    public void CreateArticleVariation_Should_SetVariation()
    {
        var fromArticle = Article.CreateArticle(new Name("Base article"));

        var article = Article.CreateArticleVariation(new Name("Variation article"), fromArticle);

        Assert.True(article.IsVariation);
        Assert.Equal(fromArticle.Guid, article.Variation?.FromArticleGuid);
    }

    [Fact]
    public void CreateArticlePack_Should_SetPack()
    {
        var fromArticle = Article.CreateArticle(new Name("Base article"));
        var quantity = 10.Amount();

        var article = Article.CreateArticlePack(new Name("Pack article"), fromArticle, quantity);

        Assert.True(article.IsPack);
        Assert.Equal(fromArticle.Guid, article.Pack?.FromArticleGuid);
        Assert.Equal(quantity, article.Pack?.Quantity);
    }

    [Fact]
    public void CreateArticleKit_Should_SetKit()
    {
        var fromArticle = Article.CreateArticle(new Name("Base article"));
        var component = new ArticleKitComponent(fromArticle, 2.Amount());

        var article = Article.CreateArticleKit(new Name("Kit article"), [component]);

        Assert.True(article.IsKit);
        Assert.Same(component, article.Kit?.Components.Single());
    }

    [Fact]
    public void ArticleKitComponentRequest_Should_RejectEmptyArticleGuid()
    {
        Assert.Throws<ArgumentException>(
            () => new ArticleKitComponentRequest(Guid.Empty, 2.Amount()));
    }

    [Fact]
    public void ArticleKitComponentRequest_Should_RejectNonPositiveQuantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ArticleKitComponentRequest(Guid.NewGuid(), 0.Amount()));
    }

    [Fact]
    public void ArticleKitComponentRequest_Should_SetArticleGuidAndQuantity()
    {
        var articleGuid = Guid.NewGuid();
        var quantity = 2.Amount();

        var component = new ArticleKitComponentRequest(articleGuid, quantity);

        Assert.Equal(articleGuid, component.ArticleGuid);
        Assert.Equal(quantity, component.Quantity);
    }

    [Fact]
    public void ArticleKitComponent_Should_RejectNonPositiveQuantity()
    {
        var article = Article.CreateArticle(new Name("Base article"));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ArticleKitComponent(article, 0.Amount()));
    }

    [Fact]
    public void ArticleKitComponent_Should_SetArticleAndQuantity()
    {
        var article = Article.CreateArticle(new Name("Base article"));
        var quantity = 2.Amount();

        var component = new ArticleKitComponent(article, quantity);

        Assert.Same(article, component.Article);
        Assert.Equal(article.Guid, component.ArticleGuid);
        Assert.Equal(quantity, component.Quantity);
    }

    [Fact]
    public void CreateArticleContainer_Should_SetContainer()
    {
        var article = Article.CreateArticleContainer(new Name("Container article"));

        Assert.True(article.IsContainer);
        Assert.Null(article.Container?.MaxMass);
    }

    [Fact]
    public void Rename_Should_OnlyChangeName()
    {
        var article = Article.CreateArticleContainer(new Name("Test article"));

        article.Rename(new Name("Renamed article"));

        Assert.Equal("Renamed article", article.Name.Value);
        Assert.Null(article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.None, article.ModificationTypes);
    }

    [Fact]
    public void MarkModificationType_Should_AccumulateModificationTypes()
    {
        var article = Article.CreateArticle(new Name("Test article"));

        article.MarkModificationType(ArticleModifiedType.General);
        article.MarkModificationType(ArticleModifiedType.MetricsChanged);

        Assert.Equal(ArticleModifiedType.General | ArticleModifiedType.MetricsChanged, article.ModificationTypes);
        Assert.True(article.IsEditStarted);
    }

    [Fact]
    public void MarkModificationType_Should_ResetPreviousModificationTypes_When_EditSessionStarts()
    {
        var article = Article.CreateArticle(new Name("Test article"));

        article.MarkModificationType(ArticleModifiedType.General);
        SetIsEditStarted(article, false);
        article.MarkModificationType(ArticleModifiedType.MetricsChanged);

        Assert.Equal(ArticleModifiedType.MetricsChanged, article.ModificationTypes);
    }

    [Fact]
    public void MarkAsEditedBy_Should_SetActor()
    {
        var article = Article.CreateArticle(new Name("Test article"));
        var actorGuid = Guid.NewGuid();

        article.MarkAsEditedBy(actorGuid);

        Assert.Equal(actorGuid, article.EditedByGuid);
    }

    [Fact]
    public void ContainerMutation_Should_UpdateContainer()
    {
        var article = Article.CreateArticle(new Name("Test article"));

        article.SetContainerMaxMass(Mass.FromKilograms(10));

        Assert.Equal(Mass.FromKilograms(10), article.Container?.MaxMass);
    }

    [Fact]
    public void PartitionMutation_Should_UpdatePartitions()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var article = Article.CreateArticle(new Name("Test article"));

        article.AddPartition(partition);

        Assert.Contains(partition, article.Partitions);
    }

    private static void SetIsEditStarted(Article article, bool value)
    {
        var property = typeof(Article).GetProperty(nameof(Article.IsEditStarted))!;

        property.SetValue(article, value);
    }
}
