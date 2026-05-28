using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Repositories;

public sealed class PartitionedRepositoryFilterTests
{
    [Fact]
    public async Task ArticleGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = CreateArticle("Public article");
        var firstPartitionEntity = CreateArticle("First article", partitions: [firstPartition]);
        var secondPartitionEntity = CreateArticle("Second article", partitions: [secondPartition]);
        context.Articles.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ArticleRepository(context);

        await AssertFilterCombinations(
            (partitions, notChildOfAnyPartition) => repository.GetManyInfo(
                AllRows,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task ArticleGetInfoByGuid_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = CreateArticle("Public article");
        var firstPartitionEntity = CreateArticle("First article", partitions: [firstPartition]);
        var secondPartitionEntity = CreateArticle("Second article", partitions: [secondPartition]);
        context.Articles.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ArticleRepository(context);

        await AssertGetInfoByGuidFilterCombinations(
            (guid, partitions, notChildOfAnyPartition) => repository.GetInfoByGuid(
                guid,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task ArticleGetInfoByBarcode_Should_ApplyPartitionFilter()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var articleService = new ArticleService(new ArticleRepository(context));
        var publicEntity = CreateArticle("Public article");
        var firstPartitionEntity = CreateArticle("First article", partitions: [firstPartition]);
        var secondPartitionEntity = CreateArticle("Second article", partitions: [secondPartition]);
        await articleService.SetCode128(new Code128("PUBLIC-123"), publicEntity, CreateDomainActor());
        await articleService.SetEan13(new Ean13("7891234567895"), firstPartitionEntity, CreateDomainActor());
        await articleService.SetEan13(new Ean13("7891234567896"), secondPartitionEntity, CreateDomainActor());
        context.Articles.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ArticleRepository(context);

        var accessible = await repository.GetInfoByBarcode(
            new Barcode("7891234567895", BarcodeFormat.Ean13),
            childOfAnyOfThesePartitions: [firstPartition.Guid],
            notChildOfAnyPartition: true);
        var inaccessible = await repository.GetInfoByBarcode(
            new Barcode("7891234567896", BarcodeFormat.Ean13),
            childOfAnyOfThesePartitions: [firstPartition.Guid],
            notChildOfAnyPartition: true);
        var publicArticle = await repository.GetInfoByBarcode(
            new Barcode("PUBLIC-123", BarcodeFormat.Code128),
            childOfAnyOfThesePartitions: [firstPartition.Guid],
            notChildOfAnyPartition: true);

        Assert.Equal(firstPartitionEntity.Guid, accessible?.Guid);
        Assert.Null(inaccessible);
        Assert.Equal(publicEntity.Guid, publicArticle?.Guid);
    }

    [Fact]
    public async Task ItemGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var article = CreateArticle("Article");
        var publicEntity = Item.CreateItem(article);
        var firstPartitionEntity = Item.CreateItem(article);
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = Item.CreateItem(article);
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Articles.Add(article);
        context.Items.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);

        await AssertFilterCombinations(
            (partitions, notChildOfAnyPartition) => repository.GetManyInfo(
                AllRows,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task ItemGetInfoByGuid_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var article = CreateArticle("Article");
        var publicEntity = Item.CreateItem(article);
        var firstPartitionEntity = Item.CreateItem(article);
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = Item.CreateItem(article);
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Articles.Add(article);
        context.Items.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);

        await AssertGetInfoByGuidFilterCombinations(
            (guid, partitions, notChildOfAnyPartition) => repository.GetInfoByGuid(
                guid,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = CreateUser("public-user");
        var firstPartitionEntity = CreateUser("first-user");
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = CreateUser("second-user");
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Users.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        await AssertFilterCombinations(
            (partitions, notChildOfAnyPartition) => repository.GetManyInfo(
                AllRows,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGetInfoByGuid_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = CreateUser("public-user");
        var firstPartitionEntity = CreateUser("first-user");
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = CreateUser("second-user");
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Users.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        await AssertGetInfoByGuidFilterCombinations(
            (guid, partitions, notChildOfAnyPartition) => repository.GetInfoByGuid(
                guid,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGroupGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = UserGroup.CreateUserGroup(new Nameid("public-group"));
        var firstPartitionEntity = UserGroup.CreateUserGroup(new Nameid("first-group"));
        firstPartitionEntity.AddPartition(firstPartition);
        var secondPartitionEntity = UserGroup.CreateUserGroup(new Nameid("second-group"));
        secondPartitionEntity.AddPartition(secondPartition);
        context.UserGroups.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserGroupRepository(context);

        await AssertFilterCombinations(
            (partitions, notChildOfAnyPartition) => repository.GetManyInfo(
                AllRows,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGroupGetInfoByGuid_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = UserGroup.CreateUserGroup(new Nameid("public-group"));
        var firstPartitionEntity = UserGroup.CreateUserGroup(new Nameid("first-group"));
        firstPartitionEntity.AddPartition(firstPartition);
        var secondPartitionEntity = UserGroup.CreateUserGroup(new Nameid("second-group"));
        secondPartitionEntity.AddPartition(secondPartition);
        context.UserGroups.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserGroupRepository(context);

        await AssertGetInfoByGuidFilterCombinations(
            (guid, partitions, notChildOfAnyPartition) => repository.GetInfoByGuid(
                guid,
                childOfAnyOfThesePartitions: partitions,
                notChildOfAnyPartition: notChildOfAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    private static readonly Pagination AllRows = new(Page.FirstPage, Limit.MaxLimit);

    private static Article CreateArticle(
        string name,
        IReadOnlyCollection<Partition>? partitions = null)
    {
        var article = Article.CreateArticle(new Name(name), CreateDomainActor());

        foreach (var partition in partitions ?? [])
        {
            article.AddPartition(partition, CreateDomainActor());
        }

        return article;
    }

    private static async Task AssertFilterCombinations<T>(
        Func<IReadOnlyCollection<Guid>?, bool?, Task<IReadOnlyCollection<T>>> query,
        Guid publicEntityGuid,
        Guid firstPartitionEntityGuid,
        Guid secondPartitionEntityGuid,
        Guid firstPartitionGuid)
        where T : class
    {
        AssertGuids([publicEntityGuid, firstPartitionEntityGuid, secondPartitionEntityGuid], await query(null, null));
        AssertGuids([publicEntityGuid], await query(null, true));
        AssertGuids([firstPartitionEntityGuid, secondPartitionEntityGuid], await query(null, false));
        AssertGuids([firstPartitionEntityGuid], await query([firstPartitionGuid], null));
        AssertGuids([publicEntityGuid, firstPartitionEntityGuid], await query([firstPartitionGuid], true));
        AssertGuids([], await query([], null));
        AssertGuids([publicEntityGuid], await query([], true));
    }

    private static async Task AssertGetInfoByGuidFilterCombinations<T>(
        Func<Guid, IReadOnlyCollection<Guid>?, bool?, Task<T?>> query,
        Guid publicEntityGuid,
        Guid firstPartitionEntityGuid,
        Guid secondPartitionEntityGuid,
        Guid firstPartitionGuid)
        where T : class
    {
        AssertGuid(publicEntityGuid, await query(publicEntityGuid, null, null));
        Assert.Null(await query(firstPartitionEntityGuid, null, true));
        Assert.Null(await query(publicEntityGuid, [firstPartitionGuid], null));
        AssertGuid(firstPartitionEntityGuid, await query(firstPartitionEntityGuid, [firstPartitionGuid], null));
        AssertGuid(publicEntityGuid, await query(publicEntityGuid, [firstPartitionGuid], true));
        AssertGuid(firstPartitionEntityGuid, await query(firstPartitionEntityGuid, [firstPartitionGuid], true));
        Assert.Null(await query(secondPartitionEntityGuid, [firstPartitionGuid], true));
    }

    private static void AssertGuids<T>(IReadOnlyCollection<Guid> expected, IReadOnlyCollection<T> actual)
        where T : class
    {
        var actualGuids = actual
            .Select(GetGuid)
            .Order()
            .ToArray();

        Assert.Equal(expected.Order(), actualGuids);
    }

    private static void AssertGuid<T>(Guid expected, T? actual)
        where T : class
    {
        Assert.NotNull(actual);
        Assert.Equal(expected, GetGuid(actual));
    }

    private static Guid GetGuid<T>(T dto)
        => dto switch
        {
            Fargo.Application.Articles.ArticleDto article => article.Guid,
            Fargo.Application.Items.ItemDto item => item.Guid,
            Fargo.Application.Users.UserDto user => user.Guid,
            Fargo.Application.UserGroups.UserGroupDto userGroup => userGroup.Guid,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unsupported DTO type.")
        };

    private static FargoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FargoDbContext(options);
    }

    private static (Partition First, Partition Second) AddPartitions(FargoDbContext context)
    {
        var first = Partition.CreatePartition(new Name("First partition"));
        var second = Partition.CreatePartition(new Name("Second partition"));
        context.Partitions.AddRange(first, second);
        return (first, second);
    }

    private static User CreateUser(string nameid)
        => User.CreateUser(
            new Nameid(nameid),
            new PasswordHash(new string('h', PasswordHash.MinLength)));
}
