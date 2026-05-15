using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using UnitsNet;

namespace Fargo.Core.Tests.Articles;

public sealed class ArticleMutationTests
{
    [Fact]
    public void Constructor_Should_SetActorAndGeneralModificationType()
    {
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);

        var article = new Article(new Name("Test article"), actor);

        Assert.Equal(actor.Guid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.General, article.ModificationTypes);
    }

    [Fact]
    public void Rename_Should_SetActorAndGeneralModificationType()
    {
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);
        var article = new Article(new Name("Test article"), actor);

        article.StartEdit(actor);
        article.Rename(new Name("Renamed article"));

        Assert.Equal("Renamed article", article.Name.Value);
        Assert.Equal(actor.Guid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.General, article.ModificationTypes);
    }

    [Fact]
    public void Mutations_Should_AccumulateModificationTypes()
    {
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);
        var article = new Article(new Name("Test article"), actor);

        article.StartEdit(actor);
        article.Rename(new Name("Renamed article"));
        article.SetMetrics(Mass.FromKilograms(1), null, null, null);

        Assert.Equal(ArticleModifiedType.General | ArticleModifiedType.Metrics, article.ModificationTypes);
    }

    [Fact]
    public void StartEdit_Should_ResetPreviousModificationTypes()
    {
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);
        var article = new Article(new Name("Test article"), actor);

        article.StartEdit(actor);
        article.Rename(new Name("Renamed article"));
        article.StartEdit(actor);
        article.SetMetrics(Mass.FromKilograms(1), null, null, null);

        Assert.Equal(ArticleModifiedType.Metrics, article.ModificationTypes);
    }

    [Fact]
    public void Mutation_Should_Throw_WhenNoActorWasEverProvided()
    {
        var article = new Article();

        Assert.Throws<ArticleEditNotStartedFargoDomainException>(
            () => article.Rename(new Name("Renamed article")));
    }

    [Fact]
    public void ContainerMutation_Should_MarkContainerModificationType()
    {
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);
        var article = new Article(new Name("Test article"), actor);

        article.StartEdit(actor);
        article.SetContainerMaxMass(Mass.FromKilograms(10));

        Assert.Equal(ArticleModifiedType.Container, article.ModificationTypes);
        Assert.Equal(Mass.FromKilograms(10), article.Container?.MaxMass);
    }

    [Fact]
    public void PartitionMutation_Should_MarkPartitionModificationType()
    {
        var partition = new Partition { Name = new Name("Restricted") };
        var actor = TestActor.WithPermissions(ActionType.CreateArticle, ActionType.EditArticle);
        var article = new Article(new Name("Test article"), actor);

        article.StartEdit(actor);

        article.AddPartition(partition);

        Assert.Contains(partition, article.Partitions);
        Assert.Equal(ArticleModifiedType.Partition, article.ModificationTypes);
    }

    private static class TestActor
    {
        public static Actor WithPermissions(params ActionType[] permissions)
            => new(
                Guid.NewGuid(),
                isAdmin: false,
                isActive: true,
                permissionActions: permissions,
                partitionAccessesGuids: []);
    }
}
