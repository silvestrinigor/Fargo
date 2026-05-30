using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Core.Shared;
using NSubstitute;

namespace Fargo.Core.Tests.Identity;

public sealed class EntityActorAccessValidationTests
{
    [Fact]
    public void ArticleEdit_Should_RejectActorWithoutArticleAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        article.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.EditArticle]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => article.ChangeDescription(new Description("Description"), actor));
    }

    [Fact]
    public void ArticleVariationCreate_Should_RejectActorWithoutSourceArticleAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var sourceArticle = Article.CreateArticle(new Name("Source article"), CreateDomainActor());
        sourceArticle.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.CreateArticle]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => Article.CreateArticleVariation(new Name("Variation"), sourceArticle, actor));
    }

    [Fact]
    public void ItemCreate_Should_RejectActorWithoutSourceArticleAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        article.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.CreateItem]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => Item.CreateItem(article, actor));
    }

    [Fact]
    public async Task ItemMoveToContainer_Should_RejectActorWithoutParentContainerAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var parent = Item.CreateItem(Article.CreateArticleContainer(new Name("Container"), CreateDomainActor()));
        parent.AddPartition(partition, CreateDomainActor());
        var member = Item.CreateItem(Article.CreateArticle(new Name("Member"), CreateDomainActor()));
        var actor = CreateActor([ActionType.EditItem]);
        var service = new ItemService(Substitute.For<IItemRepository>());

        await Assert.ThrowsAsync<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => service.MoveToContainer(parent, member, actor));
    }

    [Fact]
    public void PartitionEdit_Should_RejectActorWithoutPartitionAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var actor = CreateActor([ActionType.EditPartition]);

        Assert.Throws<UserPartitionAccessNotAuthorizedFargoDomainException>(
            () => partition.Rename(new Name("Renamed"), actor));
    }

    [Fact]
    public void UserEdit_Should_RejectActorWithoutUserAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var user = User.CreateUser(new Nameid("valid-user"), new PasswordHash(new string('a', 60)));
        user.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.EditUser]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => user.ChangeDescription(new Description("Description"), actor));
    }

    [Fact]
    public void UserGroupAssignment_Should_RejectActorWithoutUserGroupAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var user = User.CreateUser(new Nameid("valid-user"), new PasswordHash(new string('a', 60)));
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        userGroup.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.EditUser]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => user.AddUserGroup(userGroup, actor));
    }

    [Fact]
    public void UserGroupEdit_Should_RejectActorWithoutUserGroupAccess()
    {
        var partition = Partition.CreatePartition(new Name("Restricted"));
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        userGroup.AddPartition(partition, CreateDomainActor());
        var actor = CreateActor([ActionType.EditUserGroup]);

        Assert.Throws<UserEntityAccessNotAuthorizedFargoDomainException>(
            () => userGroup.ChangeDescription(new Description("Description"), actor));
    }

    [Fact]
    public void UserGroupDeleteValidation_Should_RejectActorParentGroup()
    {
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        var actor = CreateActor([ActionType.DeleteUserGroup]);

        Assert.Throws<UserCannotDeleteParentUserGroupFargoDomainException>(
            () => UserGroupService.ValidateUserGroupDelete(userGroup, actor, [userGroup.Guid]));
    }

    private static Actor CreateActor(IReadOnlyCollection<ActionType> permissions, IReadOnlyCollection<Guid>? partitionAccesses = null)
        => new(
            Guid.NewGuid(),
            isAdmin: false,
            isActive: true,
            permissions,
            partitionAccesses ?? []);
}
