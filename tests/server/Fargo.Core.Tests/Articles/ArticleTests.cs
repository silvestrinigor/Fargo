using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
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
    public void GetEntityType_ShouldBeArticle()
    {
        var article = Article.NewArticle(Aname);

        Assert.Equal(Fargo.Core.Entities.EntityType.Article, article.GetEntityType());
    }
}
