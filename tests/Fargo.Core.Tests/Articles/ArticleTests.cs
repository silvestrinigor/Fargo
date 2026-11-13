using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Core.Tests.Articles;

public class ArticleTests
{
    private static readonly Name Aname = new("Widget");

    [Fact]
    public void NewArticle_ShouldBeDefaultType()
    {
        var article = Article.NewArticle(Aname);

        Assert.Equal(ArticleType.Default, article.ArticleType);
        Assert.Equal(Aname, article.Name);
    }

    [Fact]
    public void NewArticle_ShouldAlwaysBelongToGlobalPartition()
    {
        var article = Article.NewArticle(Aname);

        Assert.Contains(FargoCoreWellKnowGuids.GlobalPartitionGuid, article.PartitionGuids);
    }

    [Fact]
    public void NewArticleContainer_ShouldBeContainerType()
    {
        var article = Article.NewArticleContainer(Aname);

        Assert.Equal(ArticleType.Container, article.ArticleType);
        Assert.NotNull(article.Container);
    }

    [Fact]
    public void NewArticleContainer_WithMaxMass_ShouldSetMaxMass()
    {
        var article = Article.NewArticleContainer(Aname, maxMass: Mass.FromKilograms(25));

        Assert.NotNull(article.Container!.MaxMass);
        Assert.Equal(25, article.Container!.MaxMass!.Value.Kilograms, 3);
    }

    [Fact]
    public void NewArticleContainer_WithAllowedArticles_ShouldListThemWithMaxQuantity()
    {
        var a1 = Article.NewArticle(new Name("A1"));
        var a2 = Article.NewArticle(new Name("A2"));

        var container = Article.NewArticleContainer(
            Aname,
            allowedArticles: [(a1, Scalar.FromAmount(10)), (a2, Scalar.FromAmount(5))]);

        Assert.Equal(2, container.Container!.AllowedArticles.Count);
        Assert.Contains(container.Container.AllowedArticles, a => a.AllowedArticleGuid == a1.Guid && a.MaxQuantity!.Value.Value == 10);
        Assert.Contains(container.Container.AllowedArticles, a => a.AllowedArticleGuid == a2.Guid && a.MaxQuantity!.Value.Value == 5);
    }

    [Fact]
    public void NewArticleContainer_WhenAllowedArticleIsItself_ShouldThrow()
    {
        var container = Article.NewArticleContainer(Aname);

        // Adding the container to its own allow-list must be rejected.
        Assert.Throws<FargoCoreException>(
            () => container.Container!.AddAllowedArticle(container, Scalar.FromAmount(1)));
    }

    [Fact]
    public void NewArticleContainer_WhenAllowedArticleMaxQuantityIsNotPositive_ShouldThrow()
    {
        var allowed = Article.NewArticle(new Name("A1"));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Article.NewArticleContainer(Aname, allowedArticles: [(allowed, Scalar.FromAmount(0))]));
    }

    [Fact]
    public void NewArticleContainer_WhenAllowedArticleHasNoMaxQuantity_ShouldBeAllowedWithoutCap()
    {
        var allowed = Article.NewArticle(new Name("A1"));

        var container = Article.NewArticleContainer(Aname, allowedArticles: [(allowed, null)]);

        var entry = Assert.Single(container.Container!.AllowedArticles);
        Assert.Equal(allowed.Guid, entry.AllowedArticleGuid);
        Assert.Null(entry.MaxQuantity);
    }

    [Fact]
    public void NewArticleContainer_WhenMaxQuantitySuppliedForEventTrackedArticle_ShouldThrow()
    {
        var eventTracked = Article.NewArticle(new Name("Bulk"), ArticleTrackingMode.Event);

        Assert.Throws<FargoCoreException>(
            () => Article.NewArticleContainer(Aname, allowedArticles: [(eventTracked, Scalar.FromAmount(5))]));
    }

    [Fact]
    public void NewArticleContainer_WhenEventTrackedArticleAllowedWithoutMaxQuantity_ShouldBeAllowed()
    {
        var eventTracked = Article.NewArticle(new Name("Bulk"), ArticleTrackingMode.Event);

        var container = Article.NewArticleContainer(Aname, allowedArticles: [(eventTracked, null)]);

        var entry = Assert.Single(container.Container!.AllowedArticles);
        Assert.Equal(eventTracked.Guid, entry.AllowedArticleGuid);
        Assert.Null(entry.MaxQuantity);
    }

    [Fact]
    public void NewArticleContainer_WhenAllowedArticleDuplicated_ShouldThrow()
    {
        var allowed = Article.NewArticle(new Name("A1"));

        Assert.Throws<FargoCoreException>(
            () => Article.NewArticleContainer(
                Aname,
                allowedArticles: [(allowed, Scalar.FromAmount(1)), (allowed, Scalar.FromAmount(2))]));
    }

    [Fact]
    public void NewArticleVariation_ShouldBeVariationOfSource()
    {
        var source = Article.NewArticle(new Name("Source"));

        var variation = Article.NewArticleVariation(Aname, source);

        Assert.Equal(ArticleType.Variation, variation.ArticleType);
        Assert.NotNull(variation.Variation);
        Assert.Equal(source.Guid, variation.Variation!.FromArticleGuid);
    }

    [Fact]
    public void NewArticlePack_ShouldBePackOfSourceWithQuantity()
    {
        var source = Article.NewArticle(new Name("Source"));

        var pack = Article.NewArticlePack(Aname, source, Scalar.FromAmount(6));

        Assert.Equal(ArticleType.Pack, pack.ArticleType);
        Assert.NotNull(pack.Pack);
        Assert.Equal(source.Guid, pack.Pack!.FromArticleGuid);
    }

    [Fact]
    public void NewArticleKit_ShouldContainComponents()
    {
        var c1 = Article.NewArticle(new Name("C1"));
        var c2 = Article.NewArticle(new Name("C2"));

        var kit = Article.NewArticleKit(Aname, [(c1, Scalar.FromAmount(1)), (c2, Scalar.FromAmount(2))]);

        Assert.Equal(ArticleType.Kit, kit.ArticleType);
        Assert.Equal(2, kit.KitComponents.Count);
    }

    [Fact]
    public void AddPartition_WhenNew_ShouldAssociate()
    {
        var article = Article.NewArticle(Aname);
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        article.AddPartition(partition);

        Assert.Contains(partition.Guid, article.PartitionGuids);
    }

    [Fact]
    public void AddPartition_WhenAlreadyAssociated_ShouldBeIdempotent()
    {
        var article = Article.NewArticle(Aname);
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        article.AddPartition(partition);
        article.AddPartition(partition);

        Assert.Single(article.PartitionGuids, g => g == partition.Guid);
    }

    [Fact]
    public void AddPartition_WhenNull_ShouldThrow()
    {
        var article = Article.NewArticle(Aname);

        Assert.Throws<ArgumentNullException>(() => article.AddPartition(null!));
    }

    [Fact]
    public void RemovePartition_WhenGlobal_ShouldThrow()
    {
        var article = Article.NewArticle(Aname);

        var ex = Assert.Throws<FargoCoreException>(
            () => article.RemovePartition(FargoCoreWellKnowGuids.GlobalPartitionGuid));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void RemovePartition_WhenAssociated_ShouldRemove()
    {
        var article = Article.NewArticle(Aname);
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);
        article.AddPartition(partition);

        article.RemovePartition(partition.Guid);

        Assert.DoesNotContain(partition.Guid, article.PartitionGuids);
    }

    [Fact]
    public void SetMetrics_ShouldComputeVolumeFromDimensions()
    {
        var article = Article.NewArticle(Aname);

        article.SetMetrics(
            Mass.FromKilograms(2),
            Length.FromMeters(1),
            Length.FromMeters(2),
            Length.FromMeters(3));

        Assert.NotNull(article.Volume);
        Assert.Equal(6, article.Volume!.Value.CubicMeters, 3);
    }

    [Fact]
    public void EntityType_ShouldBeArticle()
    {
        var article = Article.NewArticle(Aname);

        Assert.Equal(Entities.EntityType.Article, article.EntityType);
    }
}
