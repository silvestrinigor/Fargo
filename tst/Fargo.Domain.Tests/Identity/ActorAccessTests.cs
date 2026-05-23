using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using Fargo.Core.Users;

namespace Fargo.Core.Tests.Identity;

public sealed class ActorAccessTests
{
    [Fact]
    public void ValidateHasAccess_Should_AllowPublicPartitionedEntity()
    {
        var article = Article.CreateArticle(new Name("Public article"), CreateDomainActor());
        var actor = CreateActor();

        var exception = Record.Exception(() => actor.ValidateHasAccess(article));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateHasAccess_Should_AllowEntityWithMatchingPartition()
    {
        var partition = Partition.CreatePartition(new Name("Allowed"));
        var article = Article.CreateArticle(new Name("Partitioned article"), CreateDomainActor());
        article.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor(partition.Guid);

        var exception = Record.Exception(() => actor.ValidateHasAccess(article));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateHasAccess_Should_AllowAdminActor()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var article = Article.CreateArticle(new Name("Partitioned article"), CreateDomainActor());
        article.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor(isAdmin: true);

        var exception = Record.Exception(() => actor.ValidateHasAccess(article));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateHasAccess_Should_Throw_WhenActorHasNoEntityPartitionAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var article = Article.CreateArticle(new Name("Partitioned article"), CreateDomainActor());
        article.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor();

        var exception = Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => actor.ValidateHasAccess(article));

        Assert.Equal(actor.Guid, exception.UserGuid);
        Assert.Equal(article.Guid, exception.EntityGuid);
    }

    [Fact]
    public void ValidateHasPartitionAccess_Should_Throw_WhenActorHasNoPartitionAccess()
    {
        var partitionGuid = Guid.NewGuid();
        var actor = CreateActor();

        var exception = Assert.Throws<UserPartitionAccessNotAuthorizedFargoDomainException>(
            () => actor.ValidateHasPartitionAccess(partitionGuid));

        Assert.Equal(actor.Guid, exception.UserGuid);
        Assert.Equal(partitionGuid, exception.PartitionGuid);
    }

    private static Actor CreateActor(params Guid[] partitionAccesses)
        => CreateActor(isAdmin: false, partitionAccesses);

    private static Actor CreateActor(bool isAdmin, params Guid[] partitionAccesses)
        => new(
            Guid.NewGuid(),
            isAdmin,
            isActive: true,
            [],
            partitionAccesses);
}
