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
    public async Task ArticleGetInfoByBarcode_Should_ApplyPartitionFilter()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var articleService = new ArticleService(new ArticleRepository(context));
        var publicEntity = CreateArticle("Public article");
        var firstPartitionEntity = CreateArticle("First article", partitions: [firstPartition]);
        var secondPartitionEntity = CreateArticle("Second article", partitions: [secondPartition]);
        await articleService.SetCode128(new Code128("PUBLIC-123"), publicEntity);
        await articleService.SetEan13(new Ean13("7891234567895"), firstPartitionEntity);
        await articleService.SetEan13(new Ean13("7891234567896"), secondPartitionEntity);
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
        var publicEntity = new Item(article);
        var firstPartitionEntity = new Item(article);
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = new Item(article);
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
    public async Task UserGroupGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = new UserGroup { Nameid = new Nameid("public-group") };
        var firstPartitionEntity = new UserGroup { Nameid = new Nameid("first-group") };
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = new UserGroup { Nameid = new Nameid("second-group") };
        secondPartitionEntity.Partitions.Add(secondPartition);
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

    private static readonly Pagination AllRows = new(Page.FirstPage, Limit.MaxLimit);

    private static Article CreateArticle(
        string name,
        IReadOnlyCollection<Partition>? partitions = null)
    {
        var article = Article.CreateArticle(new Name(name));

        foreach (var partition in partitions ?? [])
        {
            article.AddPartition(partition);
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

    private static void AssertGuids<T>(IReadOnlyCollection<Guid> expected, IReadOnlyCollection<T> actual)
        where T : class
    {
        var actualGuids = actual
            .Select(GetGuid)
            .Order()
            .ToArray();

        Assert.Equal(expected.Order(), actualGuids);
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
        var first = new Partition { Name = new Name("First partition") };
        var second = new Partition { Name = new Name("Second partition") };
        context.Partitions.AddRange(first, second);
        return (first, second);
    }

    private static User CreateUser(string nameid)
        => new()
        {
            Nameid = new Nameid(nameid),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
}
